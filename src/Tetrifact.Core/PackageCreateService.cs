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

        private readonly ILock _lock;

        private readonly IFileSystem _filesystem;

        #endregion

        #region CTORS

        public PackageCreateService(IIndexReadService indexReader, ILockProvider lockProvider, IArchiveService archiveService, ISettings settings, ILogger<IPackageCreateService> log, IPackageCreateWorkspace workspace, IHashService hashService, IFileSystem filesystem)
        {
            _indexReader = indexReader;
            _log = log;
            _filesystem = filesystem;
            _archiveService = archiveService;
            _workspace = workspace;
            _settings = settings;
            
            _hashService = hashService;
            _lock = lockProvider.Instance;
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
                _log.LogInformation("Package create started");

                if (!_settings.AllowPackageCreate)
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
                _lock.Lock(newPackage.Id);

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

                _log.LogInformation("Hashing package content");

                // populate hash collection with paths of incoming files, this ensures we maintain order 
                // when parallel processing hashes for these files
                IDictionary<string, string> hashes = new Dictionary<string, string>();
                foreach (string file in files)
                    hashes.Add(file, null);

                if (newPackage.ExistingFiles != null)
                    foreach(ManifestItem item in newPackage.ExistingFiles)
                        hashes[item.Path] = item.Hash;

                // write incoming files to repo, get hash of each
                files.AsParallel().ForAll(delegate(string filePath) {
                    try 
                    {
                        if (existingFiles.Contains(filePath)){ 
                            FileOnDiskProperties filePropertiesOnDisk = _indexReader.GetRepositoryFileProperties(filePath, hashes[filePath]);
                            if (filePropertiesOnDisk == null)
                                throw new Exception($"Expected local file {filePath} @ hash {hashes[filePath]} does not exist");

                            _workspace.SubscribeToHash(filePath, hashes[filePath], newPackage.Id, filePropertiesOnDisk.Size, false);
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
                        // give exception more context, AsParallel should pass exception back up to waiting parenting thread
                        throw new Exception($"Error processing hash for file {filePath}", ex);
                    }
                });

                _workspace.Manifest.Description = newPackage.Description;

                _log.LogInformation("Writing package manifest");

                // calculate package hash from child hashes
                _workspace.WriteManifest(newPackage.Id, _hashService.FromString(string.Join(string.Empty, hashes.Values)));

                _workspace.Dispose();

                if (_settings.AutoCreateArchiveOnPackageCreate){
                    _log.LogInformation("Generating package archive");
                    _archiveService.EnsurePackageArchive(newPackage.Id);
                }

                return new PackageCreateResult { Success = true, PackageHash = _workspace.Manifest.Hash };
            }
            finally
            {
                if (!string.IsNullOrEmpty(newPackage.Id))
                    _lock.Unlock(newPackage.Id);
            }
        }

        #endregion
    }
}
