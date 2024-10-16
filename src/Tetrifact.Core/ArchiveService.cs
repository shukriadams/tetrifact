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
            return Path.Combine(_settings.ArchiveQueuePath, $"{packageId}.json");
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

        private async Task Archive7Zip(string packageId, string archivePathTemp)
        {
            // create staging directory
            string tempDir1 = Path.Join(_settings.TempPath, $"__repack_{packageId}");
            string tempDir2 = Path.Join(_settings.TempPath, $"_repack_{packageId}");

            const int bufSize = 6024;

            Manifest manifest = _indexReader.GetManifest(packageId);

            // copy all files to single Directory
            if (!Directory.Exists(tempDir2))
            {
                _log.LogInformation($"Archive generation : gathering files for package {packageId}");
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
                                // copy async not used here because cannot get this delegate to block asParallel, 
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
                            // copy async not used here because cannot get this delegate to block asParallel, 
                            StreamsHelper.Copy(fileStream, writeStream, bufSize); 
                    }

                    counter++;

                    if (cacheUpdateIncrements == 0 || counter % cacheUpdateIncrements == 0)
                    {
                        _log.LogInformation($"Gathering file {counter}/{manifest.Files.Count}, package \"{packageId}\".");
                        string progressCacheKey = this.GetArchiveProgressKey(packageId);
                        ArchiveProgressInfo progress = _cache.Get<ArchiveProgressInfo>(progressCacheKey);
                        if (progress != null)
                        {
                            progress.FileCopyProgress = ((decimal)counter / (decimal)manifest.Files.Count) * 100;
                            _cache.Set(progressCacheKey, progress);
                        }
                    }
                });

                Directory.Move(tempDir1, tempDir2);
            }

            _log.LogInformation($"Archive generation : building archive for  package {packageId}");

            // force delete temp file if it already exists, this can sometimes fail and we want an exception to be thrown to block 7zip being called.
            // if 7zip encounted
            if (_fileSystem.File.Exists(archivePathTemp))
                _fileSystem.File.Delete(archivePathTemp);

            DateTime compressStart = DateTime.Now;

            // ensure bin path exists
            if (!_fileSystem.File.Exists(_settings.ExternaArchivingExecutable))
                throw new Exception($"7zip binary not found at specified path \"{_settings.ExternaArchivingExecutable}\".");

            _log.LogInformation($"Invoking 7z archive generation for package \"{packageId}\".");

            // -aoa swtich forces overwriting of existing zip file should it exist
            string command = $"{_settings.ExternaArchivingExecutable} -aoa a -tzip -mx={_settings.ArchiveCPUThreads} -mmt=on {archivePathTemp} {tempDir2}/*";
            ShellResult result = Shell.Run(command, false, 3600000); // set timeout to 1 hour
            TimeSpan compressTaken = DateTime.Now - compressStart;

            if (result.ExitCode == 0)
            {
                _log.LogInformation($"Archive comression with 7zip complete, took {Math.Round(compressTaken.TotalSeconds, 0)} seconds.");
                if (result.StdErr.Any())
                    _log.LogError($"Archive comression with 7zip succeeded, but with errors. Took {Math.Round(compressTaken.TotalSeconds, 0)} seconds. {string.Join("", result.StdErr)}");
            }
            else
            {
                _log.LogError($"Archive comression with 7zip failed, took {Math.Round(compressTaken.TotalSeconds, 0)} seconds. {string.Join("", result.StdErr)}");
            }
        }

        private async Task ArchiveDotNetZip(string packageId, string archivePathTemp)
        {
            DateTime compressStart = DateTime.Now;
            
            _log.LogInformation($"Starting archive generation for package {packageId}. Type: .Net compression. Rate : {_settings.DownloadArchiveCompression}.");

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
            _log.LogInformation($"Archive comression with default dotnet ZipArchive complete, took {Math.Round(compressTaken.TotalSeconds, 0)} seconds.");
        }

        public async Task CreateNextQueuedArchive() 
        {
            ArchiveQueueInfo archiveQueueInfo = null;
            string progressCacheKey = null;
            ArchiveProgressInfo progress = null;

            foreach (string queuedFile in _fileSystem.Directory.GetFiles(_settings.ArchiveQueuePath))
            {
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
                    continue;
                }

                progressCacheKey = this.GetArchiveProgressKey(archiveQueueInfo.PackageId);
                progress = _cache.Get<ArchiveProgressInfo>(progressCacheKey);
                if (progress == null)
                {
                    _log.LogError($"Progress object not found for archive generation package {archiveQueueInfo.PackageId}, this should not happen.");
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
            _cache.Set(progressCacheKey, progress);

            await this.CreateArchive(archiveQueueInfo.PackageId);

            progress.State = PackageArchiveCreationStates.Processed_CleanupRequired;
            _cache.Set(progressCacheKey, progress);
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
                if (_settings.ArchivingMode == ArchivingModes.SevenZip)
                    await Archive7Zip(packageId, archivePathTemp);
                else
                    await ArchiveDotNetZip(packageId, archivePathTemp);
                
                // flip temp file to final path, it is ready for use only when this happens
                _fileSystem.File.Move(archivePathTemp, archivePath);
                TimeSpan totalTaken = DateTime.Now - totalStart;
                _log.LogInformation($"Archive generation : package {packageId} complete, total time {Math.Round(totalTaken.TotalSeconds, 0)} seconds.");
            }
            catch(Exception ex) 
            { 
                _log.LogError($"Package archive for {packageId} failed unexpectedly with {ex}");
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
                    _log.LogError($"Corrupt queue file {queuedFile}, content is \n\n{queueFileContent}\n\n. Error is: {ex}. Force deleting queued file.");
                    try
                    {
                        _fileSystem.File.Delete(queuedFile);
                    }
                    catch (Exception ex2)
                    {
                        _log.LogError($"Failed to delete corrupt queue file {queuedFile}. Error is: {ex2}.");
                    }
                    continue;
                }

                progressKey = this.GetArchiveProgressKey(archiveQueueInfo.PackageId);
                progress = _cache.Get<ArchiveProgressInfo>(progressKey);
                if (progress == null)
                {
                    _log.LogError($"Progress object not found for archive generation package {archiveQueueInfo.PackageId}, this should not happen.");
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
