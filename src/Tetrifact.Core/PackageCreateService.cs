using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        private readonly IProcessManager _repositoryLocks;

        #endregion

        #region CTORS

        public PackageCreateService(IIndexReadService indexReader, IProcessManagerFactory processManagerFactory, IArchiveService archiveService, ISettings settings, ILogger<IPackageCreateService> log, IPackageCreateWorkspace workspace, IHashService hashService)
        {
            _indexReader = indexReader;
            _log = log;
            _archiveService = archiveService;
            _workspace = workspace;
            _settings = settings;
            _hashService = hashService;
            _repositoryLocks = processManagerFactory.GetInstance(ProcessManagerContext.Repository);
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifest"></param>
        PackageCreateResult IPackageCreateService.Create(PackageCreateArguments newPackage)
        {
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

                // if new package is archive, ensure correct file count
                if (newPackage.IsArchive && newPackage.Files.Count() != 1)
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidFileCount };

                // check if there is absolute space available on storage backend. SpaceSafetyThreshold is zero by default
                // therefore has no effect if not set. This is also a disk % free type check, which might be too
                // rudimentary. There is an additional and newer space limit further down.
                DiskUseStats useStats = _indexReader.GetDiskUseSats();
                if (useStats.ToPercent() <= _settings.SpaceSafetyThreshold)
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.OutOfSpace };

                // calculate if repository is at max size. Note that we don't yet know the size of the incoming package,
                // that check is done further down, but if we can detect we're at capacity already, we can bug out early.                
                long repositorySize = 0;
                if (_settings.MaxRepositorySize.HasValue)
                {
                    foreach (string packageId in _indexReader.GetAllPackageIds())
                    {
                        Manifest manifest = _indexReader.GetManifestHead(packageId);
                        repositorySize += manifest.SizeOnDisk;
                    }
                    
                    if (repositorySize > _settings.MaxRepositorySize)
                        return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.OutOfSpace };
                }
 
                // lock repository while doing upload, this is to prevent parallel deletes etc from other processes.
                // That sort of thing would be bad.
                _repositoryLocks.AddUnique(newPackage.Id, false);

                _workspace.Initialize();

                // if new package is archive, unzip
                if (newPackage.IsArchive)
                    _workspace.AddArchiveContent(newPackage.Files.First().Content);
                else
                    foreach (PackageCreateItem formFile in newPackage.Files)
                        _workspace.AddIncomingFile(formFile.Content, formFile.FileName);

                // get all files which were uploaded, sort alphabetically for combined hashing
                IEnumerable<string> files = _workspace.GetIncomingFileNames().ToList();
                
                // if merging with existing files, add those files to files collection
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

                long newPackageSizeOnDisk = 0;
                
                // write incoming files to repo, get hash of each
                files.AsParallel().ForAll(delegate(string filePath) {
                    try 
                    {
                        count ++;
                        if (count % stepSize == 0)
                            _log.LogDebug($"Processing file {count}/{files.Count()}, package \"{newPackage.Id}\".");
                
                        if (existingFiles.Contains(filePath))
                        { 
                            // handle if incoming file is in a partial upload, ie, an uploaded that has been deduped on the client before uploading.

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
                        } 
                        else
                        {
                            // handle if incoming file is not in a partial upload, ie, all package files are in the uploaded package and
                            // we dedupe here on the server.
                            
                            FileOnDiskProperties fileProperties = _workspace.GetIncomingFileProperties(filePath);

                            lock(hashes)
                            {
                                // 2 hashes are stored here, the hash of the path, AND the hash of the content at that path
                                hashes[filePath] = _hashService.FromString(filePath) + fileProperties.Hash;
                            }
                        
                            RepositoryAddResponse response = _workspace.WriteFile(filePath, fileProperties.Hash, fileProperties.Size, newPackage.Id);
                            newPackageSizeOnDisk += response.SizeOnDisk;
                        }
                    }
                    catch (Exception ex)
                    {
                        lock(errors)
                            errors.Add($"Error processing hash for file {filePath} {ex}");
                    }
                });

                // check if new incoming files would exceed allowed max repo size. Abort new package if so. This still leaves all incoming files
                // on disk, but these will be removed in next clean cycle.
                if (_settings.MaxRepositorySize.HasValue && newPackageSizeOnDisk + repositorySize > _settings.MaxRepositorySize)
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.OutOfSpace };

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
                    _repositoryLocks.RemoveUnique(newPackage.Id);
            }
        }

        #endregion
    }
}
