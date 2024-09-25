using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tetrifact.Core
{
    public class IndexReadService : IIndexReadService
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<IIndexReadService> _log;

        private readonly ITagsService _tagService;

        private readonly IFileSystem _fileSystem;
        
        private readonly IHashService _hashService;

        private readonly IProcessLockManager _lock;

        private readonly IMemoryCache _cache;

        #endregion

        #region CTORS

        public IndexReadService(ISettingsProvider settingsProvider, IMemoryCache cache, ITagsService tagService, ILogger<IIndexReadService> log, IFileSystem fileSystem, IHashService hashService, IProcessLockManager lockInstance)
        {
            _settings = settingsProvider.Get();
            _tagService = tagService;
            _log = log;
            _cache = cache;
            _fileSystem = fileSystem;
            _hashService = hashService;
            _lock = lockInstance;
        }

        #endregion

        #region METHODS

        public void Initialize()
        {
            if (_settings.WipeTempOnStart)
            {
                if (Directory.Exists(_settings.TempPath))
                    try
                    {
                        _fileSystem.Directory.Delete(_settings.TempPath, true);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning($"Error attemtping to purge TempPath on app start, ignoring. {ex}");
                    }
            }
            else
            {
                _log.LogInformation("Temp dir wipe disabled, skipping");
            }

            // force purge archive queue
            try
            {
                _fileSystem.Directory.Delete(_settings.ArchiveQueuePath, true);
            }
            catch (Exception ex)
            {
                _log.LogWarning($"Error attemtping to purge ArchiveQueuePath on app start, ignoring. {ex}");
            }

            // force recreate all again
            _fileSystem.Directory.CreateDirectory(_settings.ArchivePath);
            _fileSystem.Directory.CreateDirectory(_settings.ArchiveQueuePath);
            _fileSystem.Directory.CreateDirectory(_settings.PackagePath);
            _fileSystem.Directory.CreateDirectory(_settings.TempPath);
            _fileSystem.Directory.CreateDirectory(_settings.RepositoryPath);
            _fileSystem.Directory.CreateDirectory(_settings.TagsPath);
            _fileSystem.Directory.CreateDirectory(_settings.MetricsPath);
            _fileSystem.Directory.CreateDirectory(_settings.PackageDiffsPath);
        }

        bool IIndexReadService.PackageExists(string packageId)
        {
            return ((IIndexReadService)this).GetManifest(packageId) != null;
        }

        void IIndexReadService.WriteManifest(string packageId, Manifest manifest)
        {
            string targetFolder = Path.Join(_settings.PackagePath, packageId);
            string packageTempPath = Path.Join(targetFolder, "~manifest.json");
            string packageHeadTempPath = Path.Join(targetFolder, "~manifest-head.json");
            string packagePath = Path.Join(targetFolder, "manifest.json");
            string packageHeadPath = Path.Join(targetFolder, "manifest-head.json");

            // calculate package hash from child hashes
            _fileSystem.Directory.CreateDirectory(targetFolder);
            _fileSystem.File.WriteAllText(packageTempPath, JsonConvert.SerializeObject(manifest));
        

            // head is package minus file data
            Manifest headCopy = JsonConvert.DeserializeObject<Manifest>(JsonConvert.SerializeObject(manifest));
            headCopy.Files = new List<ManifestItem>();
            _fileSystem.File.WriteAllText(packageHeadTempPath, JsonConvert.SerializeObject(headCopy));

            // flip temp files
            if (_fileSystem.File.Exists(packagePath))
                _fileSystem.File.Delete(packagePath);

            _fileSystem.File.Move(packageTempPath, packagePath);

            if (_fileSystem.File.Exists(packageHeadPath))
                _fileSystem.File.Delete(packageHeadPath);

            _fileSystem.File.Move(packageHeadTempPath, packageHeadPath);
        }

        void IIndexReadService.UpdatePackageCreateDate(string packageId, string createdUtcDate)
        {
            DateTime created;
            
            try 
            {
                created = DateTime.Parse(createdUtcDate);
            }
            catch(Exception)
            { 
                throw new FormatException($"{createdUtcDate} could not be parsed into a datetime");
            }

            Manifest manifest = ((IIndexReadService)this).GetManifest(packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            manifest.CreatedUtc = created;

            ((IIndexReadService)this).WriteManifest(packageId, manifest);
        }

        IEnumerable<string> IIndexReadService.GetAllPackageIds()
        {
            IEnumerable<string> rawList = _fileSystem.Directory.GetDirectories(_settings.PackagePath);
            return rawList.Select(r => Path.GetFileName(r));
        }

        bool IIndexReadService.PackageNameInUse(string id)
        {
            string packagePath = Path.Join(_settings.PackagePath, id);
            return _fileSystem.Directory.Exists(packagePath);
        }

        Manifest IIndexReadService.GetExpectedManifest(string packageId)
        { 
            Manifest manifest = ((IIndexReadService)this).GetManifest(packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            return manifest;
        }

        FileOnDiskProperties IIndexReadService.GetRepositoryFileProperties(string path, string hash)
        { 
            string filePath = Path.Join(_settings.RepositoryPath, path, hash, "bin");
            if (!File.Exists(filePath))
                return null;

            FileInfo fileInfo = new FileInfo(filePath);

            return new FileOnDiskProperties { Hash = hash, Size = fileInfo.Length };
        }

        Manifest IIndexReadService.GetManifest(string packageId)
        { 
            return this.GetManifest(packageId, "manifest.json");
        }

        Manifest IIndexReadService.GetManifestHead(string packageId)
        {
            return this.GetManifest(packageId, "manifest-head.json");
        }

        private Manifest GetManifest(string packageId, string type)
        {
            string filePath = Path.Join(_settings.PackagePath, packageId, type);
            if (!_fileSystem.File.Exists(filePath))
                return null;

            try
            {
                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(_fileSystem.File.ReadAllText(filePath));
                var allTags = _tagService.GetPackagesThenTags();
                if (allTags.ContainsKey(packageId))
                    manifest.Tags = allTags[packageId].ToHashSet();

                return manifest;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unexpected error trying to parse JSON from manifest @ {filePath}. File is likely corrupt.");
                return null;
            }
        }

        string IIndexReadService.GetFileAbsolutePath(IPackageFile item)
        {
            return Path.Combine(_settings.RepositoryPath, item.Path, item.Hash, "bin");
        }

        GetFileResponse IIndexReadService.GetFile(string id)
        {
            FileIdentifier fileIdentifier = FileIdentifier.Decloak(id);
            string directFilePath = ((IIndexReadService)this).GetFileAbsolutePath(fileIdentifier);
            
            if (_fileSystem.File.Exists(directFilePath))
                return new GetFileResponse(new FileStream(directFilePath, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(fileIdentifier.Path));

            return null;
        }

        (bool, string) IIndexReadService.VerifyPackage(string packageId) 
        {
            Manifest manifest = ((IIndexReadService)this).GetManifest(packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            StringBuilder hashes = new StringBuilder();
            IEnumerable<string> files = _hashService.SortFileArrayForHashing(manifest.Files.Select(r => r.Path).ToArray());
            List<string> errors = new List<string>();

            foreach (string filePath in files)
            {
                ManifestItem manifestItem = manifest.Files.FirstOrDefault(r => r.Path == filePath);

                string directFilePath = Path.Combine(_settings.RepositoryPath, manifestItem.Path, manifestItem.Hash, "bin");
                if (_fileSystem.File.Exists(directFilePath))
                {
                    FileOnDiskProperties fileOnDiskProperties = _hashService.FromFile(directFilePath);
                    if (fileOnDiskProperties.Hash != manifestItem.Hash)
                        errors.Add($"Package file {manifestItem.Path} expects hash {manifestItem.Hash} but on-disk has {fileOnDiskProperties.Hash}");
                    else 
                        hashes.Append(_hashService.FromString(manifestItem.Path) + fileOnDiskProperties.Hash);
                }
                else
                {
                    errors.Add($"Package file {directFilePath} could not be found.");
                }
            }

            if (errors.Any())
                return (false, string.Join(",", errors));

            string finalHash = _hashService.FromString(hashes.ToString());
            if (finalHash != manifest.Hash)
                return (false, $"Actual package hash {finalHash} does not match expected manifest hash {manifest.Hash}");

            return (true, string.Empty);
        }

        void IIndexReadService.DeletePackage(string packageId)
        {
            if (!_settings.AllowPackageDelete)
                throw new OperationNowAllowedException();

            Manifest manifest = ((IIndexReadService)this).GetManifest(packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            // delete repo entries for this package, the binary will be removed by a cleanup job
            foreach (ManifestItem item in manifest.Files)
            {
                string targetPath = Path.Combine(_settings.RepositoryPath, item.Path, item.Hash, "packages", packageId);
                if (_fileSystem.File.Exists(targetPath))
                    _fileSystem.File.Delete(targetPath);
            }

            // delete package folder
            string packageFolder = Path.Combine(_settings.PackagePath, packageId);
            if (_fileSystem.Directory.Exists(packageFolder))
                // NOTE : this will fail if user does not have permission to delete a file in dir (linux) or if file is marked as readonly (win). 
                // todo : on exception try to determine cause
                _fileSystem.Directory.Delete(packageFolder, true);

            // delete archives for package
            string archivePath = Path.Combine(_settings.ArchivePath, $"{packageId}.zip");
            if (_fileSystem.File.Exists(archivePath))
            {
                try
                {
                    if (_lock.IsLocked(archivePath))
                    {
                        _log.LogWarning($"Failed to purge archive {archivePath}, assuming in use. Will attempt delete on next pass.");
                    }
                    else
                    {
                        _fileSystem.File.Delete(archivePath);
                    }
                }
                catch (IOException ex)
                {
                    // ignore these, file is being downloaded, it will eventually be nuked by routine cleanup
                    _log.LogError($"Failed to purge archive {archivePath}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }

            // delete tag links for package
            string[] tagFiles = _fileSystem.Directory.GetFiles(_settings.TagsPath, packageId, SearchOption.AllDirectories);
            foreach (string tagFile in tagFiles)
            {
                try
                {
                    _fileSystem.File.Delete(tagFile);
                }
                catch (IOException ex)
                {
                    // ignore these, file is being downloaded, it will eventually be nuked by routine cleanup
                    _log.LogWarning($"Failed to delete tag {tagFile}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }
        }

        DiskUseStats IIndexReadService.GetDiskUseSats()
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DriveInfo drive = new DriveInfo(path);
            DiskUseStats stats = new DiskUseStats();

            stats.TotalBytes = drive.TotalSize;
            stats.FreeBytes = drive.AvailableFreeSpace;

            return stats;
        }

        PartialPackageLookupResult IIndexReadService.FindExisting(PartialPackageLookupArguments newPackage)
        {
            IList<ManifestItem> existingFileList = new List<ManifestItem>();
            
            bool errors = false;

            // write incoming files to repo, get hash of each
            newPackage.Files.AsParallel().ForAll(delegate (ManifestItem file) {
                string repositoryPathDir = string.Empty;

                try
                {
                    repositoryPathDir = _fileSystem.Path.Join(_settings.RepositoryPath, file.Path, file.Hash);
                    if (_fileSystem.Directory.Exists(repositoryPathDir))
                    {
                        // block file from cleanup for 1 hour
                        _cache.Set($"{repositoryPathDir}::LOOKUP_RESERVE::", true, new DateTimeOffset(DateTime.UtcNow.AddHours(1)));

                        lock(existingFileList)
                            existingFileList.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    // give exception more context, AsParallel should pass exception back up to waiting parenting thread
                    _log.LogError($"Error looking up existing repo file {repositoryPathDir} {file} {ex}");
                    errors = true;
                }
            });

            // throw latest exception, if any were raised
            if (errors)
                throw new Exception("An error occurred trying to read from local file repository. Please check logs for more info");

            existingFileList = existingFileList.OrderBy(f => f.Path).ToList();

            return new PartialPackageLookupResult
            {
                Files = existingFileList
            };
        }

        #endregion
    }
}
