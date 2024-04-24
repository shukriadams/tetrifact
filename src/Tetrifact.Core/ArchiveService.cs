using Microsoft.Extensions.Logging;
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

        #region METHODs

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

        public string GetPackageArchiveTempPath(string packageId)
        {
            return Path.Combine(_settings.ArchivePath, $"{packageId}.zip.tmp");
        }

        public virtual Stream GetPackageAsArchive(string packageId)
        {
            string archivePath = this.GetPackageArchivePath(packageId);

            // trigger archive creation
            if (!_fileSystem.File.Exists(archivePath))
                this.CreateArchive(packageId);

            // wait for archive to be built
            string tempPath = this.GetPackageArchiveTempPath(packageId);
            DateTime start = DateTime.Now;
            TimeSpan timeout = new TimeSpan(0, 0, _settings.ArchiveWaitTimeout);

            while (_lock.IsLocked(tempPath))
            {
                if (DateTime.Now - start > timeout)
                    throw new TimeoutException($"Timed out waiting for archive {packageId} to build");

                _thread.Sleep(this._settings.ArchiveAvailablePollInterval);
            }

            _lock.Lock(ProcessLockCategories.Archive_Create, archivePath, new TimeSpan(1, 0, 0));
            return new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public int GetPackageArchiveStatus(string packageId)
        {
            if (!_indexReader.PackageExists(packageId))
                throw new PackageNotFoundException(packageId);

            string archivePath = this.GetPackageArchivePath(packageId);
            string temptPath = this.GetPackageArchiveTempPath(packageId);

            bool archiveExists = _fileSystem.File.Exists(archivePath);
            bool archiveTemptExists = _fileSystem.File.Exists(temptPath);

            // archive exists already
            if (archiveExists)
                return 2;

            // archive is being created
            if (archiveTemptExists)
                return 1;

            // neither archive nor temp file exists
            return 0;
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
            // -aoa swtich forces overwriting of existing zip file should it exist
            string command = $"{_settings.SevenZipBinaryPath} a -tzip -mx={_settings.ArchiveCPUThreads} -aoa -mmt=on {archivePathTemp} {tempDir2}/*";

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

        private void CreateArchive(string packageId)
        {
            if (!_indexReader.PackageExists(packageId))
                throw new PackageNotFoundException(packageId);

            // store path with .tmp extension while building, this is used to detect if archiving has already started
            string archivePath = this.GetPackageArchivePath(packageId);
            string archivePathTemp = this.GetPackageArchiveTempPath(packageId);
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
                // lock process - we use an in-memory lock instead of just locking the file on disk because linux file locks in 
                // Dotnet are unreliable
                _lock.Lock(ProcessLockCategories.Archive_Create, archivePathTemp);

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
