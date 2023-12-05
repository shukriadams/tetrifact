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

        private readonly ILogger<IIndexReadService> _logger;

        private readonly ITagsService _tagService;

        private readonly IFileSystem _fileSystem;
        
        private readonly IHashService _hashService;

        private readonly ILock _lock;

        #endregion

        #region CTORS

        public IndexReadService(ISettings settings, ITagsService tagService, ILogger<IIndexReadService> logger, IFileSystem fileSystem, IHashService hashService, ILockProvider lockProvider)
        {
            _settings = settings;
            _tagService = tagService;
            _logger = logger;
            _fileSystem = fileSystem;
            _hashService = hashService;
            _lock = lockProvider.Instance;
        }

        #endregion

        #region METHODS

        public void Initialize()
        {
            // wipe and recreate temp folder on app start
            if (Directory.Exists(_settings.TempPath))
                Directory.Delete(_settings.TempPath, true);

            Directory.CreateDirectory(_settings.PackagePath);
            Directory.CreateDirectory(_settings.ArchivePath);
            Directory.CreateDirectory(_settings.TempPath);
            Directory.CreateDirectory(_settings.RepositoryPath);
            Directory.CreateDirectory(_settings.TagsPath);
            Directory.CreateDirectory(_settings.MetricsPath);
            Directory.CreateDirectory(_settings.PackageDiffsPath);
        }

        public bool PackageExists(string packageId)
        {
            return this.GetManifest(packageId) != null;
        }

        public void WriteManifest(string packageId, Manifest manifest)
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

        public void UpdatePackageCreateDate(string packageId, string createdUtcDate)
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

            Manifest manifest = this.GetManifest(packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            manifest.CreatedUtc = created;

            this.WriteManifest(packageId, manifest);
        }

        public IEnumerable<string> GetAllPackageIds()
        {
            IEnumerable<string> rawList = _fileSystem.Directory.GetDirectories(_settings.PackagePath);
            return rawList.Select(r => Path.GetFileName(r));
        }

        public bool PackageNameInUse(string id)
        {
            string packagePath = Path.Join(_settings.PackagePath, id);
            return _fileSystem.Directory.Exists(packagePath);
        }

        public virtual Manifest GetExpectedManifest(string packageId)
        { 
            Manifest manifest = this.GetManifest(packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            return manifest;
        }

        public virtual FileOnDiskProperties GetRepositoryFileProperties(string path, string hash)
        { 
            string filePath = Path.Join(_settings.RepositoryPath, path, hash, "bin");
            if (!File.Exists(filePath))
                return null;

            FileInfo fileInfo = new FileInfo(filePath);

            return new FileOnDiskProperties { Hash = hash, Size = fileInfo.Length };
        }

        public virtual Manifest GetManifest(string packageId)
        { 
            return this.GetManifest(packageId, "manifest.json");
        }

        public virtual Manifest GetManifestHead(string packageId)
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
                _logger.LogError(ex, $"Unexpected error trying to parse JSON from manifest @ {filePath}. File is likely corrupt.");
                return null;
            }
        }

        public GetFileResponse GetFile(string id)
        {
            FileIdentifier fileIdentifier = FileIdentifier.Decloak(id);
            string directFilePath = Path.Combine(_settings.RepositoryPath, fileIdentifier.Path, fileIdentifier.Hash, "bin");
            
            if (_fileSystem.File.Exists(directFilePath))
                return new GetFileResponse(new FileStream(directFilePath, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(fileIdentifier.Path));

            return null;
        }

        public (bool, string) VerifyPackage(string packageId) 
        {
            Manifest manifest = this.GetManifest(packageId);
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

        public virtual void DeletePackage(string packageId)
        {
            if (!_settings.AllowPackageDelete)
                throw new OperationNowAllowedException();

            Manifest manifest = this.GetManifest(packageId);
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
                _fileSystem.Directory.Delete(packageFolder, true);

            // delete archives for package
            string archivePath = Path.Combine(_settings.ArchivePath, $"{packageId}.zip");
            if (_fileSystem.File.Exists(archivePath))
            {
                try
                {
                    if (_lock.IsLocked(archivePath))
                    {
                        _logger.LogWarning($"Failed to purge archive {archivePath}, assuming in use. Will attempt delete on next pass.");
                    }
                    else
                    {
                        _fileSystem.File.Delete(archivePath);
                    }
                }
                catch (IOException ex)
                {
                    // ignore these, file is being downloaded, it will eventually be nuked by routine cleanup
                    _logger.LogError($"Failed to purge archive {archivePath}, assuming in use. Will attempt delete on next pass. ${ex}");
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
                    _logger.LogWarning($"Failed to delete tag {tagFile}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }
        }

        public DiskUseStats GetDiskUseSats()
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DriveInfo drive = new DriveInfo(path);
            DiskUseStats stats = new DiskUseStats();

            stats.TotalBytes = drive.TotalSize;
            stats.FreeBytes = drive.AvailableFreeSpace;

            return stats;
        }

        public PartialPackageLookupResult FindExisting(PartialPackageLookupArguments newPackage)
        {
            IList<ManifestItem> existing = new List<ManifestItem>();

            // write incoming files to repo, get hash of each
            newPackage.Files.AsParallel().ForAll(delegate (ManifestItem file) {
                string repositoryPathDir = string.Empty;

                try
                {
                    repositoryPathDir = _fileSystem.Path.Join(_settings.RepositoryPath, file.Path, file.Hash);
                    if (_fileSystem.Directory.Exists(repositoryPathDir))
                        existing.Add(file);
                }
                catch (Exception ex)
                {
                    // give exception more context, AsParallel should pass exception back up to waiting parenting thread
                    throw new Exception($"Error looking up existing repo file {repositoryPathDir} {file}", ex);
                }
            });

            return new PartialPackageLookupResult
            {
                Files = existing
            };
        }

        #endregion
    }
}
