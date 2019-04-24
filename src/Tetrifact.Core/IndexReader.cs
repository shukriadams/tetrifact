using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Tetrifact.Core
{
    public class IndexReader : IIndexReader
    {
        private ITetriSettings _settings;

        private ILogger<IIndexReader> _logger;

        public IndexReader(ITetriSettings settings, ILogger<IIndexReader> logger)
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

        public IEnumerable<string> GetAllPackageIds()
        {
            IEnumerable<string> rawList = Directory.GetDirectories(_settings.PackagePath);
            return rawList.Select(r => Path.GetRelativePath(_settings.PackagePath, r));
        }

        public IEnumerable<string> GetPackageIds(int pageIndex, int pageSize)
        {
            IEnumerable<string> rawList = Directory.GetDirectories(_settings.PackagePath);
            return rawList.Skip(pageIndex).Take(pageSize).Select(r => Path.GetRelativePath(_settings.PackagePath, r));
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
                _logger.LogError(ex, $"Unexpected error trying to reading manifest @ {filePath}");
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
                    return new GetFileResponse(new FileStream(directFilePath, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path));

                return null;
            }
            catch (FormatException)
            {
                throw new InvalidFileIdException();
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

        private string GetPackageArchiveTempPath(string packageId)
        {
            return Path.Combine(_settings.ArchivePath, packageId + ".zip.tmp");
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
            // store path with .tmp extension while building, this is used to detect if archiving has already started
            string archivePath = this.GetPackageArchivePath(packageId);
            string archivePathTemp = this.GetPackageArchiveTempPath(packageId);

            // if temp archive exists, it's already building
            if (File.Exists(archivePathTemp))
                return;

            string archiveFolder = Path.GetDirectoryName(archivePathTemp);
            if (!Directory.Exists(archiveFolder))
                Directory.CreateDirectory(archiveFolder);

            // create zip file on disk asap to lock file name off
            using (FileStream zipStream = new FileStream(archivePathTemp, FileMode.Create))
            {
                Manifest manifest = this.GetManifest(packageId);
                if (manifest == null)
                {
                    // clean up first
                    zipStream.Close();
                    File.Delete(archivePathTemp);

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
                                throw new Exception($"Fatal error - item {file.Path}, package {packageId} returned a null stream");

                            await itemStream.CopyToAsync(entryStream);
                        }
                    }
                }
            }

            // flip temp file to final path, it is ready for use only when this happens
            File.Move(archivePathTemp, archivePath);
        }

        public Stream GetPackageAsArchive(string packageId)
        {
            if (!this.DoesPackageExist(packageId))
                throw new PackageNotFoundException(packageId);

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

            // create
            if (!File.Exists(archivePath))
            {
                this.CreateArchive(packageId);
                return 0;
            }

            if (!File.Exists(archivePath))
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
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error deleting archive file ${archivePath} : ${ex}");
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
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error deleting tag ${tagFile} : ${ex}");
                }
            }
        }

        public void CleanRepository()
        {
            // get a list of existing packages at time of calling. It is vital that new packages not be created
            // while clean running, they will be cleaned up as they are not on this list
            IEnumerable<string> existingPackageIds = this.GetAllPackageIds();

            CleanRepository_Internal(_settings.RepositoryPath, existingPackageIds, false);
        }

        /// <summary>
        /// Recursing method behind Clean() logic.
        /// </summary>
        /// <param name="currentDirectory"></param>
        private void CleanRepository_Internal(string currentDirectory, IEnumerable<string> existingPackageIds, bool isCurrentFolderPackages)
        {
            // todo : add max sleep time to prevent permalock
            // wait if linklock is active, something more important is busy. 
            while (LinkLock.Instance.IsLocked())
                Thread.Sleep(1000);

            string[] files = Directory.GetFiles(currentDirectory);
            string[] directories = Directory.GetDirectories(currentDirectory);

            // if no children at all, delete current node
            if (!files.Any() && !directories.Any())
                Directory.Delete(currentDirectory);

            if (isCurrentFolderPackages)
            {
                if (files.Any())
                {
                    foreach (string file in files)
                    {
                        if (!existingPackageIds.Contains(Path.GetFileName(file)))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                _logger.LogError($"Unexpected error deleting file ${file} ", ex);
                            }
                        }
                    }
                }
                else
                {
                    // if this is a package folder with no packages, it is safe to delete it and it's parent bin file
                    string binFilePath = Path.Join(Directory.GetParent(currentDirectory).FullName, "bin");
                    if (File.Exists(binFilePath))
                    {
                        try
                        {
                            File.Delete(binFilePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            _logger.LogError($"Unexpected error deleting file ${binFilePath} ", ex);
                        }
                    }

                    try
                    {
                        Directory.Delete(currentDirectory);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        _logger.LogError($"Unexpected error deleting folder ${currentDirectory} ", ex);
                    }

                    return;
                }

            }

            bool binFilePresent = files.Any(r => Path.GetFileName(r) == "bin");
            if (binFilePresent && !directories.Any())
            {
                // bin file is orphaned (no package, no package folders)
                string filePath = Path.Join(currentDirectory, "bin");

                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    _logger.LogError($"Unexpected error deleting file ${filePath} ", ex);
                }

                return;
            }

            foreach (string childDirectory in directories)
            {
                bool isPackageFolder = Path.GetFileName(childDirectory) == "packages" && binFilePresent;
                CleanRepository_Internal(childDirectory, existingPackageIds, isPackageFolder);
            }

        }
    }
}
