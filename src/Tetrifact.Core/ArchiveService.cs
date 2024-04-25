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

        private readonly IThread _thread;

        private readonly ILogger<IArchiveService> _logger;

        private readonly IFileSystem _fileSystem;

        private readonly ILock _lock;

        #endregion

        #region CTORS

        public ArchiveService(IIndexReadService indexReader, IThread thread, ILockProvider lockProvider, IFileSystem fileSystem, ILogger<IArchiveService> logger, ISettings settings)
        {
            _settings = settings;
            _thread = thread;
            _indexReader = indexReader;
            _fileSystem = fileSystem;
            _logger = logger;
            _lock = lockProvider.Instance;
        }

        #endregion

        #region METHODS

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

            // create info file
            Manifest manifest =_indexReader.GetManifest(packageId);

            // hardcode the compression factor, this needs its own calculation routine
            double compressionFactor = 0.5;

            ArchiveQueueInfo queueInfo = new ArchiveQueueInfo
            { 
                CreatedUtc = DateTime.UtcNow,
                PackageId = packageId,
                ProjectedSize = (long)Math.Round(manifest.Size * compressionFactor)
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

        public PackageArchiveCreationStatus GetPackageArchiveStatus(string packageId)
        {
            if (!_indexReader.PackageExists(packageId))
                return new PackageArchiveCreationStatus
                {
                    State = PackageArchiveCreationStates.PackageNotFound
                };

            string archivePath = this.GetPackageArchivePath(packageId);
            string archiveQueuePath = this.GetPackageArchiveQueuePath(packageId);
            bool archiveExists = _fileSystem.File.Exists(archivePath);

            // archive exists already
            if (archiveExists)
                return new PackageArchiveCreationStatus 
                {
                    State = PackageArchiveCreationStates.ArchiveAvailable     
                };

            // archive not available and not queued
            if (!_fileSystem.File.Exists(archiveQueuePath))
                return new PackageArchiveCreationStatus
                {
                    State = PackageArchiveCreationStates.ArchiveNotAvailableNotGenerated
                };

            string queuFileContent;
            ArchiveQueueInfo info;

            try
            {
                queuFileContent = _fileSystem.File.ReadAllText(archiveQueuePath);
            }
            catch (Exception ex) 
            { 
                throw new Exception($"could not read archive queue file {archiveQueuePath} for package {packageId}.", ex);
            }

            try
            {
                info = JsonConvert.DeserializeObject<ArchiveQueueInfo>(queuFileContent);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not json parse  queue file {archiveQueuePath} for package {packageId}.", ex);
            }

            string archiveTempPath = this.GetPackageArchiveTempPath(packageId);
            long tempArchiveSize = 0;

            try
            {
                FileInfo tempArchiveFileInfo = new FileInfo(archiveTempPath);
                tempArchiveSize = tempArchiveFileInfo.Length;
            }
            catch(Exception ex)
            { 
                _logger.LogWarning($"Could not read file info for temp-state archive {archiveTempPath}", ex);
                // ignore error if w
            }

            int percentDone = 0;
            if (info.ProjectedSize != 0)
                percentDone = (int)((100 * tempArchiveSize) / info.ProjectedSize);

            return new PackageArchiveCreationStatus
            {
                State = PackageArchiveCreationStates.ArchiveGenerating,
                ProgressPercent = percentDone
            };
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
                manifest.Files.AsParallel().ForAll(delegate (ManifestItem file)
                {
                    string targetPath = Path.Join(tempDir1, file.Path);

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

                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                        using (Stream fileStream = fileLookup.Content)
                        using (FileStream writeStream = new FileStream(targetPath, FileMode.Create))
                            StreamsHelper.Copy(fileStream, writeStream, bufSize);
                    }
                });

                Directory.Move(tempDir1, tempDir2);
            }

            _logger.LogInformation($"Archive generation : building archive for  package {packageId}");

            DateTime compressStart = DateTime.Now;
            string command = $"{_settings.SevenZipBinaryPath} a -tzip -mx={_settings.ArchiveCPUThreads} -mmt=on {archivePathTemp} {tempDir2}/*";

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
