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

        public ArchiveService(IIndexReadService indexReader, IMemoryCache cache, ILock lockInstance, IFileSystem fileSystem, ILogger<IArchiveService> logger, ISettings settings)
        {
            _settings = settings;
            _cache = cache;
            _indexReader = indexReader;
            _fileSystem = fileSystem;
            _logger = logger;
            _lock = lockInstance;
        }

        #endregion

        #region METHODS

        public string GetArchiveProgressKey(string packageId)
        {
            return $"archive_progress_{packageId}";
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
            // do not queu  if archive already exists
            string archivePath = this.GetPackageArchivePath(packageId);
            if (_fileSystem.File.Exists(archivePath))
                return;

            // check if queue file already exists
            string archiveQueuePath = this.GetPackageArchiveQueuePath(packageId);
            if (_fileSystem.File.Exists(archiveQueuePath))
                return;

            // create queue file
            ArchiveQueueInfo queueInfo = new ArchiveQueueInfo
            { 
                PackageId = packageId,
                QueuedUtc = DateTime.UtcNow
            };

            _fileSystem.File.WriteAllText(archiveQueuePath, JsonConvert.SerializeObject(queueInfo));

            // generate in-mem progress for archive
            Manifest manifest = _indexReader.GetManifest(packageId);

            // hardcode the compression factor, this needs its own calculation routine
            double compressionFactor = 0.6;
            ArchiveProgressInfo progress = new ArchiveProgressInfo
            {
                PackageId = packageId,
                ProjectedSize = (long)Math.Round(manifest.Size * compressionFactor),
                State = PackageArchiveCreationStates.Queued,
                QueuedUtc = queueInfo.QueuedUtc,
            };

            _cache.Set(this.GetArchiveProgressKey(packageId), progress);
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

                manifest.Files.AsParallel().WithDegreeOfParallelism(_settings.ArchiveCPUThreads).ForAll(delegate (ManifestItem file)
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

            // ensure bin path exists
            if (!_fileSystem.File.Exists(_settings.SevenZipBinaryPath))
                throw new Exception($"7zip binary not found at specified path \"{_settings.SevenZipBinaryPath}\".");

            // -aoa swtich forces overwriting of existing zip file should it exist
            string command = $"{_settings.SevenZipBinaryPath} -aoa a -tzip -mx={_settings.ArchiveCPUThreads} -mmt=on {archivePathTemp} {tempDir2}/*";

            ShellResult result = Shell.Run(command, false, 3600000); // set timeout to 1 hour
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

                            _logger.LogDebug($"Added file {file.Path} to archive");
                        }
                    }
                }
            }

            TimeSpan compressTaken = DateTime.Now - compressStart;
            _logger.LogInformation($"Archive comression with default dotnet ZipArchive complete, took {Math.Round(compressTaken.TotalSeconds, 0)} seconds.");
        }

        public void CreateNextQueuedArchive() 
        {
            ArchiveQueueInfo archiveQueueInfo = null;
            string progressKey = null;
            ArchiveProgressInfo progress = null;

            foreach (string queuedFile in _fileSystem.Directory.GetFiles(_settings.ArchiveQueuePath))
            {
                string queueFileContent = string.Empty;
                try
                {
                    queueFileContent = _fileSystem.File.ReadAllText(queuedFile);
                    archiveQueueInfo = JsonConvert.DeserializeObject<ArchiveQueueInfo>(queueFileContent);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Corrupt queue file {queuedFile}, content is \n\n{queueFileContent}\n\n. Error is: {ex}. Force deleting queued file.");
                    try
                    {
                        _fileSystem.File.Delete(queuedFile);
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError($"Failed to delete corrupt queue file {queuedFile}. Error is: {ex2}.");
                    }
                    continue;
                }

                progressKey = this.GetArchiveProgressKey(archiveQueueInfo.PackageId);
                progress = _cache.Get<ArchiveProgressInfo>(progressKey);
                if (progress == null)
                {
                    _logger.LogError($"Progress object not found for archive generation package {archiveQueueInfo.PackageId}, this should not happen.");
                    continue;
                }

                if (progress.State == PackageArchiveCreationStates.Queued)
                    break;
                else
                {
                    // force null, this var is used as flag to determine if we have anything to process
                    progress = null;
                    continue;
                }
            }

            // nothing queued, exit normally
            if (progress == null)
                return;

            progress.State = PackageArchiveCreationStates.ArchiveGenerating;
            progress.StartedUtc = DateTime.UtcNow;
            _cache.Set(progressKey, progress);

            this.CreateArchive(archiveQueueInfo.PackageId);

            progress.State = PackageArchiveCreationStates.Processed_CleanupRequired;
            _cache.Set(progressKey, progress);
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

        public void CleanupNextQueuedArchive() 
        {
            ArchiveQueueInfo archiveQueueInfo = null;
            string progressKey = null;
            ArchiveProgressInfo progress = null;
            string queueFile = null;

            foreach (string queuedFile in _fileSystem.Directory.GetFiles(_settings.ArchiveQueuePath))
            {
                queueFile = queuedFile;

                string queueFileContent = string.Empty;
                try
                {
                    queueFileContent = _fileSystem.File.ReadAllText(queuedFile);
                    archiveQueueInfo = JsonConvert.DeserializeObject<ArchiveQueueInfo>(queueFileContent);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Corrupt queue file {queuedFile}, content is \n\n{queueFileContent}\n\n. Error is: {ex}. Force deleting queued file.");
                    try
                    {
                        _fileSystem.File.Delete(queuedFile);
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError($"Failed to delete corrupt queue file {queuedFile}. Error is: {ex2}.");
                    }
                    continue;
                }

                progressKey = this.GetArchiveProgressKey(archiveQueueInfo.PackageId);
                progress = _cache.Get<ArchiveProgressInfo>(progressKey);
                if (progress == null)
                {
                    _logger.LogError($"Progress object not found for archive generation package {archiveQueueInfo.PackageId}, this should not happen.");
                    continue;
                }

                if (progress.State == PackageArchiveCreationStates.Processed_CleanupRequired)
                    break;
                else
                {
                    // force null, this var is used as flag to determine if we have anything to process
                    progress = null;
                    continue;
                }
            }

            // nothing queued, exit normally
            if (progress == null)
                return;

            // cleanup
            string tempDir2 = Path.Join(_settings.TempPath, $"_repack_{archiveQueueInfo.PackageId}");
            if (_fileSystem.Directory.Exists(tempDir2))
                _fileSystem.Directory.Delete(tempDir2, true);

            if (_fileSystem.File.Exists(queueFile))
                _fileSystem.File.Delete(queueFile);

            _cache.Remove(progressKey);
        }

        #endregion
    }
}
