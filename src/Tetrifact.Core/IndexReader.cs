using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Tetrifact.Core
{
    public class IndexReader : IIndexReader
    {
        #region FIELDS

        private readonly ITetriSettings _settings;

        private readonly ILogger<IIndexReader> _logger;

        #endregion

        #region CTORS

        public IndexReader(ITetriSettings settings, ILogger<IIndexReader> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        #endregion

        #region METHODS

        public void Initialize()
        {
            if (!Directory.Exists(_settings.PackagePath))
                Directory.CreateDirectory(_settings.PackagePath);

            if (!Directory.Exists(_settings.ArchivePath))
                Directory.CreateDirectory(_settings.ArchivePath);

            // wipe and recreate temp folder on app start
            if (Directory.Exists(_settings.TempPath))
                Directory.Delete(_settings.TempPath, true);
            Directory.CreateDirectory(_settings.TempPath);

            if (!Directory.Exists(_settings.RepositoryPath))
                Directory.CreateDirectory(_settings.RepositoryPath);

            if (!Directory.Exists(_settings.TagsPath))
                Directory.CreateDirectory(_settings.TagsPath);
        }

        public IEnumerable<string> GetAllPackageIds()
        {
            IEnumerable<string> rawList = Directory.GetDirectories(_settings.PackagePath);
            return rawList.Select(r => Path.GetRelativePath(_settings.PackagePath, r));
        }

        public IEnumerable<string> GetPackageIds(int pageIndex, int pageSize)
        {
            IEnumerable<string> rawList = Directory.GetDirectories(_settings.PackagePath);
            return rawList.Select(r => Path.GetRelativePath(_settings.PackagePath, r)).OrderBy(r => r).Skip(pageIndex).Take(pageSize);
        }

        public bool PackageNameInUse(string id)
        {
            string packagePath = Path.Join(_settings.PackagePath, id);
            return Directory.Exists(packagePath);
        }

        public Manifest GetManifest(string packageId)
        {
            string filePath = Path.Join(_settings.PackagePath, packageId, "manifest.json");
            if (!File.Exists(filePath))
                return null;

            try
            {
                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(filePath));
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

            if (File.Exists(directFilePath))
                return new GetFileResponse(new FileStream(directFilePath, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(fileIdentifier.Path));

            return null;
        }

        /// <summary>
        /// Todo : this is far too simplistic, expand to delete based on available disk space.
        /// </summary>
        public void PurgeOldArchives()
        {
            DirectoryInfo info = new DirectoryInfo(_settings.ArchivePath);

            IEnumerable<FileInfo> files = info.GetFiles().OrderByDescending(p => p.CreationTime).Skip(_settings.MaxArchives);

            foreach (FileInfo file in files)
            {
                try
                {
                    File.Delete(file.FullName);
                }
                catch (IOException ex)
                {
                    // ignore these, file might be in use, in which case we'll try to delete it next purge
                    _logger.LogWarning($"Failed to purge archive ${file}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }
        }

        public Stream GetPackageAsArchive(string packageId)
        {
            string archivePath = this.GetPackageArchivePath(packageId);

            // create
            if (!File.Exists(archivePath))
                this.CreateArchive(packageId);

            // is archive still building?
            string tempPath = this.GetPackageArchiveTempPath(packageId);
            DateTime start = DateTime.Now;
            TimeSpan timeout = new TimeSpan(0, 0, _settings.ArchiveWaitTimeout);

            while (File.Exists(tempPath))
            {
                Thread.Sleep(this._settings.ArchiveAvailablePollInterval);
                if (DateTime.Now - start > timeout)
                    throw new TimeoutException($"Timeout waiting for package ${packageId} archive to build");
            }

            return new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public int GetPackageArchiveStatus(string packageId)
        {
            if (!this.DoesPackageExist(packageId))
                throw new PackageNotFoundException(packageId);

            string archivePath = this.GetPackageArchivePath(packageId);
            string temptPath = this.GetPackageArchiveTempPath(packageId);

            // archive doesn't exist and isn't being created
            if (!File.Exists(archivePath) && !File.Exists(temptPath))
                return 0;

            if (File.Exists(temptPath))
                return 1;

            return 2;
        }

        public void DeletePackage(string packageId)
        {
            Manifest manifest = this.GetManifest(packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            // delete repo entries for this package, the binary will be removed by a cleanup job
            foreach (ManifestItem item in manifest.Files)
            {
                string targetPath = Path.Combine(_settings.RepositoryPath, item.Path, item.Hash, "packages", packageId);
                if (File.Exists(targetPath))
                    File.Delete(targetPath);
            }

            // delete package folder
            string packageFolder = Path.Combine(_settings.PackagePath, packageId);
            if (Directory.Exists(packageFolder))
                Directory.Delete(packageFolder, true);

            // delete archives for package
            string archivePath = Path.Combine(_settings.ArchivePath, packageId + ".zip");
            if (File.Exists(archivePath))
            {
                try
                {
                    File.Delete(archivePath);
                }
                catch (IOException ex)
                {
                    // ignore these, file is being downloaded, it will eventually be nuked by routine cleanup
                    _logger.LogWarning($"Failed to purge archive ${archivePath}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }

            // delete tag links for package
            string[] tagFiles = Directory.GetFiles(_settings.TagsPath, packageId, SearchOption.AllDirectories);
            foreach (string tagFile in tagFiles)
            {
                try
                {
                    File.Delete(tagFile);
                }
                catch (IOException ex)
                {
                    // ignore these, file is being downloaded, it will eventually be nuked by routine cleanup
                    _logger.LogWarning($"Failed to delete tag ${tagFile}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }
        }

        public string GetPackageArchivePath(string packageId)
        {
            return Path.Combine(_settings.ArchivePath, packageId + ".zip");
        }

        public string GetPackageArchiveTempPath(string packageId)
        {
            return Path.Combine(_settings.ArchivePath, packageId + ".zip.tmp");
        }

        private bool DoesPackageExist(string packageId)
        {
            Manifest manifest = this.GetManifest(packageId);
            return manifest != null;
        }

        private void CreateArchive(string packageId)
        {
            // store path with .tmp extension while building, this is used to detect if archiving has already started
            string archivePath = this.GetPackageArchivePath(packageId);
            string archivePathTemp = this.GetPackageArchiveTempPath(packageId);

            // if temp archive exists, it's already building
            if (File.Exists(archivePathTemp))
                return;

            if (!this.DoesPackageExist(packageId))
                throw new PackageNotFoundException(packageId);

            // create zip file on disk asap to lock file name off
            using (FileStream zipStream = new FileStream(archivePathTemp, FileMode.Create))
            {
                Manifest manifest = this.GetManifest(packageId);
                if (manifest == null)
                    throw new PackageNotFoundException(packageId);

                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in manifest.Files)
                    {
                        ZipArchiveEntry fileEntry = archive.CreateEntry(file.Path);

                        using (Stream entryStream = fileEntry.Open())
                        {
                            using (Stream itemStream = this.GetFile(file.Id).Content)
                            {
                                itemStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
            }

            // flip temp file to final path, it is ready for use only when this happens
            File.Move(archivePathTemp, archivePath);
        }

        #endregion
    }
}
