using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tetrifact.Core
{
    public class IndexReader : IIndexReader
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<IIndexReader> _logger;

        private readonly ITagsService _tagService;
        
        private readonly IHashService _hashService;

        private readonly IManagedFileSystem _filesystem;

        #endregion

        #region CTORS

        public IndexReader(ISettings settings, ITagsService tagService, ILogger<IIndexReader> logger, IManagedFileSystem filesystem, IHashService hashService)
        {
            _settings = settings;
            _tagService = tagService;
            _logger = logger;
            _filesystem = filesystem;
            _hashService = hashService;
        }

        #endregion

        #region METHODS

        public void Initialize()
        {
            // wipe and recreate temp folder on app start
            if (_filesystem.DirectoryExists(_settings.TempPath))
                _filesystem.DirectoryDelete(_settings.TempPath, true);

            _filesystem.DirectoryCreate(_settings.PackagePath);
            _filesystem.DirectoryCreate(_settings.ArchivePath);
            _filesystem.DirectoryCreate(_settings.TempPath);
            _filesystem.DirectoryCreate(_settings.RepositoryPath);
            _filesystem.DirectoryCreate(_settings.TagsPath);
            _filesystem.DirectoryCreate(_settings.PackageDiffsPath);
        }

        public IEnumerable<string> GetAllPackageIds()
        {
            IEnumerable<string> rawList = _filesystem.GetDirectories(_settings.PackagePath);
            return rawList.Select(r => Path.GetFileName(r));
        }

        public IEnumerable<string> GetPackageIds(int pageIndex, int pageSize)
        {
            IEnumerable<string> rawList = _filesystem.GetDirectories(_settings.PackagePath);
            return rawList.Select(r => Path.GetFileName(r)).OrderBy(r => r).Skip(pageIndex).Take(pageSize);
        }

        public bool PackageNameInUse(string id)
        {
            string packagePath = Path.Join(_settings.PackagePath, id);
            return _filesystem.DirectoryExists(packagePath);
        }

        public virtual Manifest GetExpectedManifest(string packageId)
        { 
            Manifest manifest = this.GetManifest(packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            return manifest;
        }

        public virtual Manifest GetManifest(string packageId)
        {
            string filePath = Path.Join(_settings.PackagePath, packageId, "manifest.json");
            if (!_filesystem.FileExists(filePath))
                return null;

            try
            {
                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(_filesystem.ReadAllText(filePath));
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
            string directFilePath = Path.Join(_settings.RepositoryPath, fileIdentifier.Path, fileIdentifier.Hash, "bin");
            
            if (_filesystem.FileExists(directFilePath))
                return new GetFileResponse(_filesystem.GetFileReadStream(directFilePath), Path.GetFileName(fileIdentifier.Path));

            return null;
        }

        public (bool, string) VerifyPackage(string packageId) 
        {
            Manifest manifest = this.GetManifest(packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            StringBuilder hashes = new StringBuilder();
            string[] files = _hashService.SortFileArrayForHashing(manifest.Files.Select(r => r.Path).ToArray());
            List<string> errors = new List<string>();

            foreach (string filePath in files)
            {
                ManifestItem manifestItem = manifest.Files.FirstOrDefault(r => r.Path == filePath);

                string directFilePath = Path.Join(_settings.RepositoryPath, manifestItem.Path, manifestItem.Hash, "bin");
                if (_filesystem.FileExists(directFilePath))
                {
                    (string, long) fileOnDiskProperties = _hashService.FromFile(directFilePath);
                    if (fileOnDiskProperties.Item1 != manifestItem.Hash)
                        errors.Add($"Package file {manifestItem.Path} expects hash {manifestItem.Hash} but on-disk has {fileOnDiskProperties.Item1}");
                    else 
                        hashes.Append(_hashService.FromString(manifestItem.Path) + fileOnDiskProperties.Item1);
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
                string targetPath = Path.Join(_settings.RepositoryPath, item.Path, item.Hash, "packages", packageId);
                if (_filesystem.FileExists(targetPath))
                    _filesystem.FileDelete(targetPath);
            }

            // delete package folder
            string packageFolder = Path.Join(_settings.PackagePath, packageId);
            if (_filesystem.DirectoryExists(packageFolder))
                _filesystem.DirectoryDelete(packageFolder, true);

            // delete archives for package
            string archivePath = Path.Join(_settings.ArchivePath, packageId + ".zip");
            if (_filesystem.FileExists(archivePath))
            {
                try
                {
                    _filesystem.FileDelete(archivePath);
                }
                catch (System.IO.IOException ex)
                {
                    // ignore these, file is being downloaded, it will eventually be nuked by routine cleanup
                    _logger.LogWarning($"Failed to purge archive ${archivePath}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }

            // delete tag links for package
            IEnumerable<string> tagFiles = _filesystem.GetFiles(_settings.TagsPath, packageId, System.IO.SearchOption.AllDirectories);
            foreach (string tagFile in tagFiles)
            {
                try
                {
                    _filesystem.FileDelete(tagFile);
                }
                catch (System.IO.IOException ex)
                {
                    // ignore these, file is being downloaded, it will eventually be nuked by routine cleanup
                    _logger.LogWarning($"Failed to delete tag ${tagFile}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }
        }

        #endregion
    }
}
