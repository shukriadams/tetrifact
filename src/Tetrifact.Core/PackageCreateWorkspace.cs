using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;

namespace Tetrifact.Core
{
    public class PackageCreateWorkspace : IPackageCreateWorkspace
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<IPackageCreateWorkspace> _log;

        private readonly IHashService _hashService;

        private readonly IFileSystem _filesystem;
        
        private readonly IIndexReadService _indexReadService;

        #endregion

        #region PROPERTIES

        public string WorkspacePath { get; private set; }

        public Manifest Manifest { get; private set; }

        #endregion

        #region CTORS

        public PackageCreateWorkspace(ISettings settings, IIndexReadService indexReadService, IFileSystem filesystem, ILogger<IPackageCreateWorkspace> log, IHashService hashService)
        {
            _indexReadService = indexReadService;
            _settings = settings;
            _log = log;
            _hashService = hashService;
            _filesystem = filesystem;
        }

        #endregion

        #region METHODS

        void IPackageCreateWorkspace.Initialize()
        {
            this.Manifest = new Manifest{ 
                IsCompressed = _settings.IsStorageCompressionEnabled
            };

            // workspace folder is super random - date now ticks + guid. We assume this is always unique and we don't check
            // this compromise is done to negate need for test coverage.
            this.WorkspacePath = Path.Join(_settings.TempPath, DateTime.UtcNow.Ticks.ToString() + Guid.NewGuid().ToString());

            // create all basic directories for a functional workspace
            _filesystem.Directory.CreateDirectory(Path.Join(this.WorkspacePath, "incoming"));
        }


        bool IPackageCreateWorkspace.AddIncomingFile(Stream formFile, string relativePath)
        {
            if (formFile.Length == 0)
                return false;
            
            string targetPath = FileHelper.ToUnixPath(Path.Join(this.WorkspacePath, "incoming", relativePath));
            _filesystem.Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                formFile.CopyTo(stream);
                return true;
            }
        }


        void IPackageCreateWorkspace.WriteFile(string filePath, string hash, long fileSize, string packageId)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash value required");

            // move file to public folder
            string targetPath = Path.Combine(_settings.RepositoryPath, filePath, hash, "bin");

            bool onDisk = false;
            string incomingPath = Path.Join(this.WorkspacePath, "incoming", filePath);

            if (!_filesystem.File.Exists(targetPath)) {

                _filesystem.Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                if (this.Manifest.IsCompressed){

                    using (FileStream zipStream = new FileStream(targetPath, FileMode.Create))
                    {
                        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                        {
                            ZipArchiveEntry fileEntry = archive.CreateEntry(filePath);
                            using (Stream entryStream = fileEntry.Open())
                            {
                                using (Stream itemStream = new FileStream(incomingPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    itemStream.CopyTo(entryStream);
                                    _log.LogInformation($"PACKAGE CREATE : placed compressed file {targetPath}");
                                }
                            }
                        }
                    }

                } else {
                    _filesystem.File.Move(incomingPath,targetPath);
                    _log.LogInformation($"PACKAGE CREATE : placed file {targetPath}");
                }

                onDisk = true;
            }

            // write package id into package subscription directory, associating it with this hash 
            ((IPackageCreateWorkspace)this).SubscribeToHash(filePath, hash, packageId, fileSize, onDisk);
        }


        // this method is called from parallel threads, make thread safe
        void IPackageCreateWorkspace.SubscribeToHash(string filePath, string hash, string packageId, long fileSize, bool onDisk) 
        {
            string packagesSubscribeDirectory = Path.Join(_settings.RepositoryPath, filePath, hash, "packages");

            _filesystem.Directory.CreateDirectory(packagesSubscribeDirectory);
            _filesystem.File.WriteAllText(Path.Join(packagesSubscribeDirectory, packageId), string.Empty);
            _log.LogInformation($"PACKAGE CREATE : subscribed package \"{packageId}\" to file \"{filePath}\", hash {hash} ");

            string pathAndHash = FileIdentifier.Cloak(filePath, hash);
            lock (this.Manifest)
            {
                this.Manifest.Files.Add(new ManifestItem { Path = filePath, Hash = hash, Id = pathAndHash });
                this.Manifest.Size += fileSize;
                if (onDisk)
                    this.Manifest.SizeOnDisk += fileSize;
            }
        }


        void IPackageCreateWorkspace.WriteManifest(string packageId, string combinedHash)
        {
            // calculate package hash from child hashes
            this.Manifest.Id = packageId;
            this.Manifest.Hash = combinedHash;

            _indexReadService.WriteManifest(packageId, this.Manifest);
        }

        
        IEnumerable<string> IPackageCreateWorkspace.GetIncomingFileNames()
        {
            IList<string> rawPaths = _filesystem.Directory.GetFiles(this.WorkspacePath, "*.*", SearchOption.AllDirectories);
            string relativeRoot = Path.Join(this.WorkspacePath, "incoming");
            return rawPaths.Select(rawPath => FileHelper.ToUnixPath(Path.GetRelativePath(relativeRoot, rawPath)));
        }


        void IPackageCreateWorkspace.AddArchiveContent(Stream file)
        {
            using (ZipArchive archive = new ZipArchive(file))
            {
                // if .Name empty it's an empty directory, this is difficult to force in testing so write as linq query to ensure coverage
                
                IEnumerable<ZipArchiveEntry> items = archive.Entries.Where(r => !string.IsNullOrEmpty(r.Name));
                int count = 0;
                int total = items.Count();

                foreach (ZipArchiveEntry entry in items)
                {
                    string targetFile = FileHelper.ToUnixPath(Path.Join(this.WorkspacePath, "incoming", entry.FullName));
                    string targetDirectory = Path.GetDirectoryName(targetFile);
                    _filesystem.Directory.CreateDirectory(targetDirectory);
                    entry.ExtractToFile(targetFile);
                    count ++;
                    _log.LogInformation($"Unpacked file ${targetFile} ({count} / {total})");
                }
            }
        }


        FileOnDiskProperties IPackageCreateWorkspace.GetIncomingFileProperties(string relativePath)
        {
            string path = Path.Join(this.WorkspacePath, "incoming", relativePath);
            return _hashService.FromFile(path);
        }


        void IPackageCreateWorkspace.Dispose()
        {
            try
            {
                if (_filesystem.Directory.Exists(this.WorkspacePath))
                    _filesystem.Directory.Delete(this.WorkspacePath, true);
            }
            catch (IOException ex)
            {
                _log.LogWarning($"Failed to delete temp folder {this.WorkspacePath}", ex);
            }
        }

        #endregion
    }
}
