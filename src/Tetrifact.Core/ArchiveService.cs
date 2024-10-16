using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public class ArchiveService : IArchiveService
    {
        #region PROPERTIES

        private readonly ISettings _settings;

        private readonly IIndexReadService _indexReader;

        private readonly ILogger<IArchiveService> _log;

        private readonly IFileSystem _fileSystem;

        private readonly IProcessLockManager _lock;

        private readonly IMemoryCache _cache;

        #endregion

        #region CTORS

        public ArchiveService(IIndexReadService indexReader, IMemoryCache cache, IProcessLockManager lockInstance, IFileSystem fileSystem, ILogger<IArchiveService> log, ISettings settings)
        {
            _settings = settings;
            _cache = cache;
            _indexReader = indexReader;
            _fileSystem = fileSystem;
            _log = log;
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
            // queue files partitioned by iso date to make string sorting easier.
            return Path.Combine(_settings.ArchiveQueuePath, $"{DateTime.UtcNow.ToIsoFSFriendly()}_{packageId}.json");
        }

        public string GetPackageArchiveTempPath(string packageId)
        {
            return Path.Combine(_settings.TempPath, $"{packageId}.zip.tmp");
        }

        public virtual void QueueArchiveCreation(string packageId)
        {
            // do not queue if archive already exists
            string archivePath = this.GetPackageArchivePath(packageId);
            if (_fileSystem.File.Exists(archivePath))
                return;

            // do not queue if queue flag for archive already exists
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

            _cache.Set(this.GetArchiveProgressKey(packageId), progress, new DateTimeOffset(DateTime.UtcNow.AddYears(1))); // don't let progress expire
            _log.LogInformation($"Queued archive creation for package \"{packageId}\".");
        }

        public virtual Stream GetPackageAsArchive(string packageId)
        {
            string archivePath = this.GetPackageArchivePath(packageId);

            // this method assume archive exists. throw error if it doesn't but this also means UI checking for archive existence has failed.
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

            string progressCacheKey = this.GetArchiveProgressKey(packageId);
            ArchiveProgressInfo cachedProgress = _cache.Get<ArchiveProgressInfo>(progressCacheKey);
            if (cachedProgress == null)
                cachedProgress = new ArchiveProgressInfo();

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
                .Skip(_settings.MaximumArchivesToKeep);

            foreach (FileInfo file in files)
            {
                if (_lock.IsLocked(file.FullName))
                {
                    // ignore these, file might be in use, in which case we'll try to delete it next purge
                    _log.LogWarning($"Failed to purge archive {file}, assuming in use. Will attempt delete on next pass.");
                    continue;
                }

                try
                {
                    _fileSystem.File.Delete(file.FullName);
                }
                catch (Exception ex)
                {
                    _log.LogWarning($"Failed to purge archive {file}, assuming in use. Will attempt delete on next pass. {ex}");
                }
            }
        }

        private async Task ArchiveDotNetZip(string packageId, string archivePathTemp)
        {
            DateTime compressStart = DateTime.Now;
            
            _log.LogInformation($"Starting archive generation for package {packageId}. Type: .Net compression. Rate : {_settings.ArchiveCompression}.");

            // create zip file on disk asap to lock file name off
            using (FileStream zipStream = new FileStream(archivePathTemp, FileMode.Create))
            {
                // Note : no null check here, we assume DoesPackageExist test above would catch invalid names
                Manifest manifest = _indexReader.GetManifest(packageId);

                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (ManifestItem file in manifest.Files)
                    {
                        ZipArchiveEntry zipEntry = archive.CreateEntry(file.Path, _settings.ArchiveCompression);

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
                                        await storageArchiveStream.CopyToAsync(zipEntryStream);
                                }
                            }
                            else
                            {
                                GetFileResponse fileLookup = _indexReader.GetFile(file.Id);
                                if (fileLookup == null)
                                    throw new Exception($"Failed to find expected package file {file.Id}- repository is likely corrupt");

                                using (Stream fileStream = fileLookup.Content)
                                    await fileStream.CopyToAsync(zipEntryStream);
                            }

                            _log.LogDebug($"Added file {file.Path} to archive");
                        }
                    }
                }
            }

            TimeSpan compressTaken = DateTime.Now - compressStart;
            _log.LogInformation($"Archive compression with default dotnet ZipArchive complete, took {Math.Round(compressTaken.TotalSeconds, 0)} seconds.");
        }

        public async Task CreateNextQueuedArchive() 
        {
            ArchiveQueueInfo archiveQueueInfo = null;
            string progressCacheKey = null;
            ArchiveProgressInfo progress = null;

            string queuedFile = _fileSystem.Directory.GetFiles(_settings.ArchiveQueuePath).OrderByDescending(f => f).FirstOrDefault();
            if (queuedFile == null)
                return;

            _log.LogInformation($"Processing archive generation for \"{queuedFile}\".");
            string queueFileContent = string.Empty;

            try
            {
                queueFileContent = _fileSystem.File.ReadAllText(queuedFile);
                archiveQueueInfo = JsonConvert.DeserializeObject<ArchiveQueueInfo>(queueFileContent);
            }
            catch (Exception ex)
            {
                _log.LogError($"Corrupt queue file {queuedFile}, content is \n\n{queueFileContent}\n\n. Error is: {ex}. Force deleting queued file.");
                try
                {
                    _fileSystem.File.Delete(queuedFile);
                }
                catch (Exception ex2)
                {
                    _log.LogError($"Failed to delete corrupt queue file {queuedFile}. Error is: {ex2}.");
                }
                return;
            }

            progressCacheKey = this.GetArchiveProgressKey(archiveQueueInfo.PackageId);
            progress = _cache.Get<ArchiveProgressInfo>(progressCacheKey);
            if (progress == null) 
                progress = new ArchiveProgressInfo 
                {
                    PackageId = archiveQueueInfo.PackageId,
                    QueuedUtc = archiveQueueInfo.QueuedUtc
                };

            progress.State = PackageArchiveCreationStates.ArchiveGenerating;
            progress.StartedUtc = DateTime.UtcNow;
            _cache.Set(progressCacheKey, progress);

            await this.CreateArchive(archiveQueueInfo.PackageId);

            progress.State = PackageArchiveCreationStates.Processed_CleanupRequired;
            _cache.Set(progressCacheKey, progress);

            // finally, cleanup queue file
            _fileSystem.File.Delete(queuedFile); 
        }

        public async Task CreateArchive(string packageId)
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
                _log.LogInformation($"Archive generation for package {packageId} skipped, existing process detected");
                return;
            }

            try
            {
                await ArchiveDotNetZip(packageId, archivePathTemp);
                
                // flip temp file to final path, it is ready for use only when this happens
                _fileSystem.File.Move(archivePathTemp, archivePath);
                TimeSpan totalTaken = DateTime.Now - totalStart;
                _log.LogInformation($"Archive generation : package {packageId} complete, total time {Math.Round(totalTaken.TotalSeconds, 0)} seconds.");
            }
            catch(Exception ex) 
            { 
                _log.LogError($"Package archive for {packageId} failed unexpectedly with {ex}.");
            }
            finally
            {
                _lock.Unlock(archivePathTemp);
            }
        }

        #endregion
    }
}
