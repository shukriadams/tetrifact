using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tetrifact.Core
{
    public class PackageCreateService : IPackageCreateService
    {
        #region FIELDS

        private readonly IIndexReadService _indexReader;

        private readonly IPackageCreateWorkspace _workspace;

        private readonly ILogger<IPackageCreateService> _log;

        private readonly ISettings _settings;

        private readonly IHashService _hashService;

        private readonly IArchiveService _archiveService;

        private readonly IProcessManager _lock;

        private readonly IFileSystem _filesystem;

        #endregion

        #region CTORS

        public PackageCreateService(IIndexReadService indexReader, IProcessManager lockInstance, IArchiveService archiveService, ISettings settings, ILogger<IPackageCreateService> log, IPackageCreateWorkspace workspace, IHashService hashService, IFileSystem filesystem)
        {
            _indexReader = indexReader;
            _log = log;
            _filesystem = filesystem;
            _archiveService = archiveService;
            _workspace = workspace;
            _settings = settings;
            _hashService = hashService;
            _lock = lockInstance;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifest"></param>
        PackageCreateResult IPackageCreateService.Create(PackageCreateArguments newPackage)
        {
            List<string> transactionLog = new List<string>();

            try
            {
                DateTime started = DateTime.Now;

                _log.LogInformation($"Package create started for package \"{newPackage.Id}\".");

                if (!_settings.PackageCreateEnabled)
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.CreateNotAllowed, PublicError = "Package creation is disabled in settings." };

                if (string.IsNullOrEmpty(newPackage.Id))
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Id is required." };

                Regex nameCheckRegx = new Regex("^[a-zA-Z0-9!._-]*$");
                if (!nameCheckRegx.Match(newPackage.Id).Success)
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidName, PublicError = $"Package name {newPackage.Id} contains invalid characters." };

                // validate the contents of "newPackage" object
                if (!newPackage.Files.Any())
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Files collection is empty." };

                // ensure package does not already exist
                if (_indexReader.PackageNameInUse(newPackage.Id))
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.PackageExists };

                // if archive, ensure correct file count
                if (newPackage.IsArchive && newPackage.Files.Count() != 1)
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidFileCount };

                // write attachments to work folder 
                long size = newPackage.Files.Sum(f => f.Content.Length);

                // prevent deletes of empty repository folders this package might need to write to
                _lock.AddUnique(ProcessCategories.Package_Create, newPackage.Id);

                _workspace.Initialize();

                // if archive, unzip
                if (newPackage.IsArchive)
                    _workspace.AddArchiveContent(newPackage.Files.First().Content);
                else
                    foreach (PackageCreateItem formFile in newPackage.Files)
                        _workspace.AddIncomingFile(formFile.Content, formFile.FileName);

                // get all files which were uploaded, sort alphabetically for combined hashing
                IEnumerable<string> files = _workspace.GetIncomingFileNames().ToList();
                
                // if merging with existing filees, add those files to files collection
                IEnumerable<string> existingFiles = new string[] { };
                if (newPackage.ExistingFiles != null)
                    existingFiles = newPackage.ExistingFiles.Select(r => r.Path);

                files = files.Concat(existingFiles);

                files = _hashService.SortFileArrayForHashing(files);

                // populate hash collection with paths of incoming files, this ensures we maintain order 
                // when parallel processing hashes for these files
                IDictionary<string, string> hashes = new Dictionary<string, string>();
                foreach (string file in files)
                    hashes.Add(file, null);

                if (newPackage.ExistingFiles != null)
                    foreach(ManifestItem item in newPackage.ExistingFiles)
                        hashes[item.Path] = item.Hash;

                List<string> errors = new List<string>();

                // we want to log some kind of progress here, but without spamming log with every file processed. Create a series of steps,
                // at which progress will be logged out
                long count = 0;
                int stepSize = 1;
                if (files.Count() > 100)
                    stepSize = (int)Math.Round((double)files.Count() / 100, 0);

                // write incoming files to repo, get hash of each
                files.AsParallel().ForAll(delegate(string filePath) {
                    try 
                    {
                        count ++;
                        if (count % stepSize == 0)
                            _log.LogDebug($"Processing file {count}/{files.Count()}, package \"{newPackage.Id}\".");

                        if (existingFiles.Contains(filePath))
                        { 
                            FileOnDiskProperties filePropertiesOnDisk = _indexReader.GetRepositoryFileProperties(filePath, hashes[filePath]);
                            if (filePropertiesOnDisk == null)
                            {
                                lock(errors)
                                    errors.Add($"Expected local file {filePath} @ hash {hashes[filePath]} does not exist");

                                return;
                            }

                            _workspace.SubscribeToHash(filePath, hashes[filePath], newPackage.Id, filePropertiesOnDisk.Size, false);
                            
                            // overwrite content hash with filepath hash + content hash, needed to calc full package hash
                            hashes[filePath] = _hashService.FromString(filePath) + hashes[filePath];
                            return;
                        }
                        
                        FileOnDiskProperties fileProperties = _workspace.GetIncomingFileProperties(filePath);

                        lock(hashes)
                        {
                            // 2 hashes are stored here, the hash of the path, AND the hash of the content at that path
                            hashes[filePath] = _hashService.FromString(filePath) + fileProperties.Hash;
                        }

                        // todo : this would be a good place to confirm that existingPackageId is actually valid
                        _workspace.WriteFile(filePath, fileProperties.Hash, fileProperties.Size, newPackage.Id);
                        
                    }
                    catch (Exception ex)
                    {
                        lock(errors)
                            errors.Add($"Error processing hash for file {filePath} {ex}");
                    }
                });

                if (errors.Any())
                    throw new Exception($"{errors.Count} errors occurred. Up to ten summarized are : {string.Join("/r", errors.Take(10))}");

                _workspace.Manifest.Description = newPackage.Description;

                // calculate package hash from child hashes
                _workspace.WriteManifest(newPackage.Id, _hashService.FromString(string.Join(string.Empty, hashes.Values)));

                _workspace.Dispose();

                if (_settings.AutoCreateArchiveOnPackageCreate){
                    _log.LogInformation($"Autogenerating archive for package \"{newPackage.Id}\".");
                    _archiveService.QueueArchiveCreation(newPackage.Id);
                }

                _log.LogInformation($"Package \"{newPackage.Id}\" created, took {(DateTime.Now - started).TotalSeconds} seconds.");

                return new PackageCreateResult { Success = true, PackageHash = _workspace.Manifest.Hash };
            }
            finally
            {
                if (newPackage.Id != null)
                    _lock.RemoveUnique(newPackage.Id);
            }
        }

        #endregion
    }
}
