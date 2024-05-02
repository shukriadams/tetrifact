using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;

namespace Tetrifact.Core
{
    public class ArchiveService : IArchiveService
    {
        #region PROPERTIES

        private readonly ISettings _settings;

        private readonly IIndexReadService _indexReader;

        private readonly ILogger<IArchiveService> _logger;

        private readonly IFileSystem _fileSystem;

        private readonly ILock _lock;

        private readonly IMemoryCache _cache;

        #endregion

        #region CTORS

        public ArchiveService(IIndexReadService indexReader, IMemoryCache cache, ILockProvider lockProvider, IFileSystem fileSystem, ILogger<IArchiveService> logger, ISettings settings)
        {
            _settings = settings;
            _cache = cache;
            _indexReader = indexReader;
            _fileSystem = fileSystem;
            _logger = logger;
            _lock = lockProvider.Instance;
        }

        #endregion

        #region METHODS

        public string GetArchiveProgressKey(string packageId)
        {
            return $"archive_progress_{packageId}";
        }

        public void EnsurePackageArchive(string packageId)
        {
            using (Stream stream = this.GetPackageAsArchive(packageId))
            {
                // createa and immediately dispose of stream
            }
        }

        public string GetPackageArchivePath(string packageId)
        {
            return Path.Combine(_settings.ArchivePath, $"{packageId}.zip");
        }

        public string GetPackageArchiveQueuePath(string packageId)
        {
            return Path.Combine(_settings.ArchiveQueuePath, $"{packageId}.json");
        }

        public string GetPackageArchiveTempPath(string packageId)
        {
            return Path.Combine(_settings.ArchivePath, $"{packageId}.zip.tmp");
        }

        public virtual void QueueArchiveCreation(string packageId)
        {
            // check if archive already exists
            string archivePath = this.GetPackageArchivePath(packageId);
            if (_fileSystem.File.Exists(archivePath))
                return;

            // check if archive creation in progress
            string archiveQueuePath = this.GetPackageArchiveQueuePath(packageId);
            if (_fileSystem.File.Exists(archiveQueuePath))
                return;

            // create info file to disk

            ArchiveQueueInfo queueInfo = new ArchiveQueueInfo
            { 
                PackageId = packageId,
                QueuedUtc = DateTime.UtcNow
            };

            _fileSystem.File.WriteAllText(archiveQueuePath, JsonConvert.SerializeObject(queueInfo));

        }

        public virtual Stream GetPackageAsArchive(string packageId)
        {
            string archivePath = this.GetPackageArchivePath(packageId);

            // trigger archive creation
            if (!_fileSystem.File.Exists(archivePath))
                throw new ArchiveNotFoundException();

            return new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public ArchiveProgressInfo GetPackageArchiveStatus(string packageId)
        {
            if (!_indexReader.PackageExists(packageId))
                return new ArchiveProgressInfo
                {
                    State = PackageArchiveCreationStates.Processed_PackageNotFound
                };

            string archivePath = this.GetPackageArchivePath(packageId);
            string archiveQueuePath = this.GetPackageArchiveQueuePath(packageId);
            bool archiveExists = _fileSystem.File.Exists(archivePath);

            // archive exists already
            if (archiveExists)
                return new ArchiveProgressInfo 
                {
                    State = PackageArchiveCreationStates.Processed_ArchiveAvailable     
                };

            // archive not available and not queued
            if (!_fileSystem.File.Exists(archiveQueuePath))
                return new ArchiveProgressInfo
                {
                    State = PackageArchiveCreationStates.Processed_ArchiveNotAvailableNotGenerated
                };

            string progressKey = this.GetArchiveProgressKey(packageId);
            ArchiveProgressInfo cachedProgress = _cache.Get<ArchiveProgressInfo>(progressKey);
            return cachedProgress;
        }

        /// <summary>
        /// Todo : this is far too simplistic, expand to delete based on available disk space.
        /// </summary>
        public void PurgeOldArchives()
        {
            DirectoryInfo info = new DirectoryInfo(_settings.ArchivePath);

            // get all existing archives, sorted by create date
            IEnumerable<FileInfo> files = info.GetFiles()
                .OrderByDescending(p => p.CreationTime)
                .Skip(_settings.MaxArchives);

            foreach (FileInfo file in files)
            {
                if (_lock.IsLocked(file.FullName))
                {
                    // ignore these, file might be in use, in which case we'll try to delete it next purge
                    _logger.LogWarning($"Failed to purge archive {file}, assuming in use. Will attempt delete on next pass.");
                    continue;
                }

                try
                {
                    _fileSystem.File.Delete(file.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to purge archive {file}, assuming in use. Will attempt delete on next pass. {ex}");
                }
            }
        }

        private void Archive7Zip(string packageId, string archivePathTemp)
        {
            // create staging directory
            string tempDir1 = Path.Join(_settings.TempPath, $"__repack_{packageId}");
            string tempDir2 = Path.Join(_settings.TempPath, $"_repack_{packageId}");

            const int bufSize = 6024;

            Manifest manifest = _indexReader.GetManifest(packageId);

            // copy all files to single Direct
            if (!Directory.Exists(tempDir2))
            {
                _logger.LogInformation($"Archive generation : gathering files for package {packageId}");
                Directory.CreateDirectory(tempDir1);
                long cacheUpdateIncrements = manifest.Files.Count / 100;
                long counter = 0;


                manifest.Files.AsParallel().ForAll(delegate (ManifestItem file)
                {
                    string targetPath = Path.Join(tempDir1, file.Path);
                    List<string> knownDirectories = new List<string>();
                    if (manifest.IsCompressed)
                    {
                        GetFileResponse fileLookup = _indexReader.GetFile(file.Id);
                        if (fileLookup == null)
                            throw new Exception($"Failed to find expected package file {file.Id} - repository is likely corrupt");

                        using (var storageArchive = new ZipArchive(fileLookup.Content))
                        {
                            ZipArchiveEntry storageArchiveEntry = storageArchive.Entries[0];
                            using (var storageArchiveStream = storageArchiveEntry.Open())
                            using (FileStream writeStream = new FileStream(targetPath, FileMode.Create))
                                StreamsHelper.Copy(storageArchiveStream, writeStream, bufSize);
                        }
                    }
                    else
                    {
                        GetFileResponse fileLookup = _indexReader.GetFile(file.Id);
                        if (fileLookup == null)
                            throw new Exception($"Failed to find expected package file {file.Id}- repository is likely corrupt");

                        string dir = Path.GetDirectoryName(targetPath);
                        if (!knownDirectories.Contains(dir)) 
                        {
                            Directory.CreateDirectory(dir);
                            knownDirectories.Add(dir);
                        }

                        // is this the fastest way of copying? benchmark
                        using (Stream fileStream = fileLookup.Content)
                        using (FileStream writeStream = new FileStream(targetPath, FileMode.Create))
                            StreamsHelper.Copy(fileStream, writeStream, bufSize);
                    }

                    counter ++;

                    if (cacheUpdateIncrements == 0 || counter % cacheUpdateIncrements == 0)
                    {
                        string key = this.GetArchiveProgressKey(packageId);
                        ArchiveProgressInfo progress = _cache.Get<ArchiveProgressInfo>(key);
                        if (progress != null)
                        {
                            progress.FileCopyProgress = ((decimal)counter / (decimal)manifest.Files.Count) * 100;
                            _cache.Set(key, progress);
                        }
                    }
                });

                Directory.Move(tempDir1, tempDir2);
            }

            _logger.LogInformation($"Archive generation : building archive for  package {packageId}");

            // force delete temp file if it already exists, this can sometimes fail and we want an exception to be thrown to block 7zip being called.
            // if 7zip encounted
            if (_fileSystem.File.Exists(archivePathTemp))
                _fileSystem.File.Delete(archivePathTemp);

            DateTime compressStart = DateTime.Now;
            string command = $"{_settings.SevenZipBinaryPath} -aoa a -tzip -mx={_settings.ArchiveCPUThreads} -mmt=on {archivePathTemp} {tempDir2}/*";

            ShellResult result = Shell.Run(command);
            TimeSpan compressTaken = DateTime.Now - compressStart;
            if (result.ExitCode == 0)
            {
                _logger.LogInformation($"Archive comression with 7zip complete, took {Math.Round(compressTaken.TotalSeconds, 0)} seconds.");
                if (result.StdErr.Any())
                    _logger.LogError($"Archive comression with 7zip succeeded, but with errors. Took {Math.Round(compressTaken.TotalSeconds, 0)} seconds. {string.Join("", result.StdErr)}");
            }
            else
            {
                _logger.LogError($"Archive comression with 7zip failed, took {Math.Round(compressTaken.TotalSeconds, 0)} seconds. {string.Join("", result.StdErr)}");
            }
        }

        private void ArchiveDefaultMode(string packageId, string archivePathTemp)
        {
            DateTime compressStart = DateTime.Now;

            // create zip file on disk asap to lock file name off
            using (FileStream zipStream = new FileStream(archivePathTemp, FileMode.Create))
            {
                // Note : no null check here, we assume DoesPackageExist test above would catch invalid names
                Manifest manifest = _indexReader.GetManifest(packageId);

                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (ManifestItem file in manifest.Files)
                    {
                        ZipArchiveEntry zipEntry = archive.CreateEntry(file.Path, _settings.DownloadArchiveCompression);

                        using (Stream zipEntryStream = zipEntry.Open())
                        {
                            if (manifest.IsCompressed)
                            {
                                GetFileResponse fileLookup = _indexReader.GetFile(file.Id);
                                if (fileLookup == null)
                                    throw new Exception($"Failed to find expected package file {file.Id} - repository is likely corrupt");

                                using (var storageArchive = new ZipArchive(fileLookup.Content))
                                {
                                    ZipArchiveEntry storageArchiveEntry = storageArchive.Entries[0];
                                    using (var storageArchiveStream = storageArchiveEntry.Open())
                                        storageArchiveStream.CopyTo(zipEntryStream);
                                }
                            }
                            else
                            {
                                GetFileResponse fileLookup = _indexReader.GetFile(file.Id);
                                if (fileLookup == null)
                                    throw new Exception($"Failed to find expected package file {file.Id}- repository is likely corrupt");

                                using (Stream fileStream = fileLookup.Content)
                                    fileStream.CopyTo(zipEntryStream);
                            }

                            _logger.LogInformation($"Added file {file.Path} to archive");
                        }
                    }
                }
            }

            TimeSpan compressTaken = DateTime.Now - compressStart;
            _logger.LogInformation($"Archive comression with default dotnet ZipArchive complete, took {Math.Round(compressTaken.TotalSeconds, 0)} seconds.");
        }

        public void CreateArchive(string packageId)
        {
            if (!_indexReader.PackageExists(packageId))
                throw new PackageNotFoundException(packageId);

            string archivePath = this.GetPackageArchivePath(packageId);
            string archivePathTemp = this.GetPackageArchiveTempPath(packageId);

            // archive already exists, no need to recreate
            if (_fileSystem.File.Exists(archivePath))
                return;

            // store path with .tmp extension while building, this is used to detect if archiving has already started
            DateTime totalStart = DateTime.Now;

            // if archive temp file exists, archive is _probably_ still being generated. To check if it is, attempt to
            // delete it. If the delete fails because file is locked, we can safely exit and wait. If it succeeds, previous
            // archive generation must have failed, and we can proceed to restart archive creation. This is crude but effective.
            if (_lock.IsLocked(archivePathTemp))
            {
                _logger.LogInformation($"Archive generation for package {packageId} skipped, existing process detected");
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(_settings.SevenZipBinaryPath))
                    ArchiveDefaultMode(packageId, archivePathTemp);
                else
                    Archive7Zip(packageId, archivePathTemp);

                // flip temp file to final path, it is ready for use only when this happens
                _fileSystem.File.Move(archivePathTemp, archivePath);
                TimeSpan totalTaken = DateTime.Now - totalStart;
                _logger.LogInformation($"Archive generation : package {packageId} complete, total time {Math.Round(totalTaken.TotalSeconds, 0)} seconds.");
            }
            finally
            {
                _lock.Unlock(archivePathTemp);
            }
        }

        #endregion
    }
}
