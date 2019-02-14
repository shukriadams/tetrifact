using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public class IndexReader : IIndexReader
    {
        private ITetriSettings _settings;

        private ILogger<IndexReader> _logger;

        public IndexReader(ITetriSettings settings, ILogger<IndexReader> logger)
        {
            _settings = settings;
            _logger = logger;
        }

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

        public IEnumerable<string> GetPackages()
        {
            IEnumerable<string> rawList = Directory.GetDirectories(_settings.PackagePath);
            return rawList.Select(r => Path.GetRelativePath(_settings.PackagePath, r));
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
                _logger.LogError(ex, string.Format("Unexpected error trying to reading manifest @ {0}", filePath));
                return null;
            }
        }

        public GetFileResponse GetFile(string id)
        {
            string path = string.Empty;
            string hash = string.Empty;

            try
            {
                string pathAndHash = Obfuscator.Decloak(id);
                Regex regex = new Regex("(.*)::(.*)");
                MatchCollection matches = regex.Matches(pathAndHash);

                if (matches.Count() != 1)
                    throw new InvalidFileIdException();

                path = matches[0].Groups[1].Value;
                hash = matches[0].Groups[2].Value;

                string directFilePath = Path.Combine(_settings.RepositoryPath, path, hash, "bin");

                if (File.Exists(directFilePath))
                    return new GetFileResponse(new FileStream(directFilePath, FileMode.Open), Path.GetFileName(path));

                return null;

            }
            catch (FormatException)
            {
                throw new InvalidFileIdException();
            }

        }

        private bool IsFileAvailable(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) { }
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Todo : this is far too simplistic, expand to delete based on available disk space.
        /// </summary>
        public void PurgeOldArchives()
        {
            DirectoryInfo info = new DirectoryInfo(_settings.ArchivePath);

            // find all files older then 10 newest ones.
            IEnumerable<FileInfo> files = info.GetFiles().OrderByDescending(p => p.CreationTime).Skip(10);

            foreach (FileInfo file in files)
            {
                try
                {
                    File.Delete(file.FullName);
                }
                catch (IOException)
                {
                    // ignore these, file might be being downloaded
                }
            }
        }

        private string GetPackageArchivePath(string packageId)
        {
            return Path.Combine(_settings.ArchivePath, packageId + ".zip");
        }

        private bool DoesPackageExist(string packageId)
        {
            try
            {
                Manifest manifest = this.GetManifest(packageId);
                return manifest != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void CreateArchive(string packageId)
        {
            FileStream zipStream = null;

            try
            {
                string archivePath = this.GetPackageArchivePath(packageId);

                string archiveFolder = Path.GetDirectoryName(archivePath);
                if (!Directory.Exists(archiveFolder))
                    Directory.CreateDirectory(archiveFolder);

                // create zip file on disk asap to lock file name off
                zipStream = new FileStream(archivePath, FileMode.Create);

                Manifest manifest = this.GetManifest(packageId);
                if (manifest == null)
                {
                    // clean up first
                    zipStream.Close();
                    File.Delete(archivePath);

                    throw new PackageNotFoundException(packageId);
                }

                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in manifest.Files)
                    {
                        ZipArchiveEntry fileEntry = archive.CreateEntry(file.Path);

                        using (Stream entryStream = fileEntry.Open())
                        {
                            Stream itemStream = this.GetFile(file.Id).Content;
                            if (itemStream == null)
                                throw new Exception(string.Format("Fatal error - item {0}, package {1} returned a null stream", file.Path, packageId));

                            await itemStream.CopyToAsync(entryStream);
                        }
                    }
                }
            }
            finally
            {
                if (zipStream != null)
                    zipStream.Close();
            }
        }

        public async Task<Stream> GetPackageAsArchiveAsync(string packageId)
        {
            if (!this.DoesPackageExist(packageId))
                throw new PackageNotFoundException(packageId);

            string archivePath = this.GetPackageArchivePath(packageId);

            // create
            if (!File.Exists(archivePath))
                this.CreateArchive(packageId);

            // if archive exists, is it done yet?
            while (!IsFileAvailable(archivePath))
                Thread.Sleep(this._settings.ArchiveAvailablePollInterval);

            return new FileStream(archivePath, FileMode.Open);
        }

        public int GetPackageArchiveStatus(string packageId)
        {
            if (!this.DoesPackageExist(packageId))
                throw new PackageNotFoundException(packageId);

            string archivePath = this.GetPackageArchivePath(packageId);

            // create
            if (!File.Exists(archivePath))
            {
                this.CreateArchive(packageId);
                return 0;
            }

            if (!IsFileAvailable(archivePath))
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
                catch (IOException)
                {
                    // ignore these, file is being downloaded, it will eventually be nuked by routine cleanup
                }
            }

            // todo : delete package tags
        }

        public void Clean()
        {
            ProcessFolder(_settings.RepositoryPath);
        }

        private void ProcessFolder(string currentDirectory)
        {
            // todo : add max sleep time to prevent permalock
            while (LinkLock.Instance.IsLocked())
                Thread.Sleep(1000);

            string[] files = Directory.GetFiles(currentDirectory);
            string[] directories = Directory.GetDirectories(currentDirectory);

            if (!files.Any() && !directories.Any())
                Directory.Delete(currentDirectory);

            if (files.Any() && !directories.Any())
                Directory.Delete(currentDirectory, true);

            foreach (string childDirectory in directories)
                ProcessFolder(childDirectory);
        }


    }
}
