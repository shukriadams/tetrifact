using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Tetrifact.Core
{
    public class ArchiveService : IArchiveService
    {
        #region PROPERTIES

        private readonly ISettings _settings;

        private readonly IIndexReader _indexReader;
        
        private readonly IThread _thread;
        
        private readonly ILogger<IArchiveService> _logger;
        
        private readonly IFileSystem _fileSystem;

        #endregion

        #region CTORS

        public ArchiveService(IIndexReader indexReader, IThread thread, IFileSystem fileSystem, ILogger<IArchiveService> logger, ISettings settings)
        {
            _settings = settings;
            _thread = thread;
            _indexReader = indexReader;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        #endregion

        #region METHODs

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
            Manifest manifest = _indexReader.GetManifest(packageId);
            return manifest != null;
        }

        public Stream GetPackageAsArchive(string packageId)
        {
            string archivePath = this.GetPackageArchivePath(packageId);

            // trigger archive creation
            if (!_fileSystem.File.Exists(archivePath))
                this.CreateArchive(packageId);

            // wait for archive to be built
            string tempPath = this.GetPackageArchiveTempPath(packageId);
            DateTime start = DateTime.Now;
            TimeSpan timeout = new TimeSpan(0, 0, _settings.ArchiveWaitTimeout);

            while (_fileSystem.File.Exists(tempPath))
            {
                if (DateTime.Now - start > timeout)
                    throw new TimeoutException($"Timed out waiting for archive ${packageId} to build");

                _thread.Sleep(this._settings.ArchiveAvailablePollInterval);
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
            if (!_fileSystem.File.Exists(archivePath) && !_fileSystem.File.Exists(temptPath))
                return 0;

            if (_fileSystem.File.Exists(temptPath))
                return 1;

            return 2;
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
                try
                {
                    _fileSystem.File.Delete(file.FullName);
                }
                catch (IOException ex)
                {
                    // ignore these, file might be in use, in which case we'll try to delete it next purge
                    _logger.LogWarning($"Failed to purge archive ${file}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }
        }

        private void CreateArchive(string packageId)
        {
            if (!this.DoesPackageExist(packageId))
                throw new PackageNotFoundException(packageId);

            // store path with .tmp extension while building, this is used to detect if archiving has already started
            string archivePath = this.GetPackageArchivePath(packageId);
            string archivePathTemp = this.GetPackageArchiveTempPath(packageId);

            // if archive temp file exists, archive is _probably_ still being generated. To check if it is, attempt to
            // delete it. If the delete fails because file is locked, we can safely exit and wait. If it succeeds, previous
            // archive generation must have failed, and we can proceed to restart archive creation. This is crude but effective.
            if (_fileSystem.File.Exists(archivePathTemp))
            {
                try
                {
                    _fileSystem.File.Delete(archivePathTemp);
                    _logger.LogInformation($"Deleted abandoned temp archive for {packageId}");
                }
                catch (IOException)
                {
                    _logger.LogInformation($"Archive generation for package {packageId} skipped, existing process detected");
                    return;
                }
            }

            _logger.LogInformation($"Archive generation for package {packageId} started");

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
                                    {
                                        storageArchiveStream.CopyTo(zipEntryStream);
                                    }
                                }
                            }
                            else
                            {
                                GetFileResponse fileLookup = _indexReader.GetFile(file.Id);
                                if (fileLookup == null)
                                    throw new Exception($"Failed to find expected package file {file.Id}- repository is likely corrupt");

                                using (Stream fileStream = fileLookup.Content)
                                {
                                    fileStream.CopyTo(zipEntryStream);
                                }
                            }

                            _logger.LogInformation($"Added file {file.Path} to archive");
                        }
                    }
                }
            }

            // flip temp file to final path, it is ready for use only when this happens
            _fileSystem.File.Move(archivePathTemp, archivePath);
            _logger.LogInformation($"Archive generation for package {packageId} complete");
        }

        #endregion
    }
}
