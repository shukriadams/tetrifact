using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Tetrifact.Core
{
    public class Workspace : IWorkspace
    {
        #region FIELDS

        private readonly ITetriSettings _settings;

        private readonly ILogger<IWorkspace> _log;

        private readonly IHashService _hashService;

        #endregion

        #region PROPERTIES

        public string WorkspacePath { get; private set; }

        public Manifest Manifest { get; private set; }

        #endregion

        #region CTORS

        public Workspace(ITetriSettings settings, ILogger<IWorkspace> log, IHashService hashService)
        {
            _settings = settings;
            _log = log;
            _hashService = hashService;
        }

        #endregion

        #region METHODS

        public void Initialize()
        {
            this.Manifest = new Manifest{ 
                IsCompressed = _settings.IsStorageCompressionEnabled
            };

            // workspaces have random names, for safety ensure name is not already in use
            while (true)
            {
                this.WorkspacePath = Path.Join(_settings.TempPath, Guid.NewGuid().ToString());
                if (!Directory.Exists(this.WorkspacePath))
                    break;
            }

            // create all basic directories for a functional workspace
            Directory.CreateDirectory(Path.Join(this.WorkspacePath, "incoming"));
        }

        public bool AddIncomingFile(Stream formFile, string relativePath)
        {
            if (formFile.Length == 0)
                return false;
            
            string targetPath = FileHelper.ToUnixPath(Path.Join(this.WorkspacePath, "incoming", relativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                formFile.CopyTo(stream);
                return true;
            }
        }

        public void WriteFile(string filePath, string hash, long fileSize, string packageId)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash value is required");

            // move file to public folder
            string targetPath = Path.Combine(_settings.RepositoryPath, filePath, hash, "bin");
            string targetDirectory = Path.GetDirectoryName(targetPath);
            string packagesSubscribeDirectory = Path.Join(targetDirectory, "packages");

            Directory.CreateDirectory(packagesSubscribeDirectory);

            bool onDisk = false;
            string incomingPath = Path.Join(this.WorkspacePath, "incoming", filePath);

            if (!File.Exists(targetPath)) {

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
                    File.Move(incomingPath,targetPath);
                    _log.LogInformation($"PACKAGE CREATE : placed file {targetPath}");
                }

                onDisk = true;
            }

            // write package id into package subscription directory, associating it with this hash 
            File.WriteAllText(Path.Join(packagesSubscribeDirectory, packageId), string.Empty);
            _log.LogInformation($"PACKAGE CREATE : subscribed package {packageId} to hash {packagesSubscribeDirectory} ");

            string pathAndHash = FileIdentifier.Cloak(filePath, hash);

            // this method is called from parallel threads, make thread safe
            lock(this.Manifest)
            {
                this.Manifest.Files.Add(new ManifestItem { Path = filePath, Hash = hash, Id = pathAndHash });
                this.Manifest.Id = packageId;
                this.Manifest.Size += fileSize;
                if (onDisk)
                    this.Manifest.SizeOnDisk += fileSize;
            }
        }

        public void WriteManifest(string packageId, string combinedHash)
        {
            // calculate package hash from child hashes
            this.Manifest.Hash = combinedHash;
            string targetFolder = Path.Join(_settings.PackagePath, packageId);
            Directory.CreateDirectory(targetFolder);
            File.WriteAllText(Path.Join(targetFolder, "manifest.json"), JsonConvert.SerializeObject(this.Manifest));

            Manifest headCopy = JsonConvert.DeserializeObject<Manifest>(JsonConvert.SerializeObject(this.Manifest));
            headCopy.Files = new List<ManifestItem>();
            File.WriteAllText(Path.Join(targetFolder, "manifest-head.json"), JsonConvert.SerializeObject(headCopy));
        }

        public IEnumerable<string> GetIncomingFileNames()
        {
            IList<string> rawPaths = Directory.GetFiles(this.WorkspacePath, "*.*", SearchOption.AllDirectories);
            string relativeRoot = Path.Join(this.WorkspacePath, "incoming");
            return rawPaths.Select(rawPath => Path.GetRelativePath(relativeRoot, rawPath));
        }

        public void AddArchiveContent(Stream file)
        {
            using (ZipArchive archive = new ZipArchive(file))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // if .Name is empty it's a directory
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    string targetFile = FileHelper.ToUnixPath(Path.Join(this.WorkspacePath, "incoming", entry.FullName));
                    string targetDirectory = Path.GetDirectoryName(targetFile);
                    Directory.CreateDirectory(targetDirectory);
                    entry.ExtractToFile(targetFile);
                }
            }
        }

        public (string, long) GetIncomingFileProperties(string relativePath)
        {
            return _hashService.FromFile(Path.Join(this.WorkspacePath, "incoming", relativePath));
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(this.WorkspacePath))
                    Directory.Delete(this.WorkspacePath, true);
            }
            catch (IOException ex)
            {
                _log.LogWarning($"Failed to delete temp folder {this.WorkspacePath}", ex);
            }
        }

        #endregion
    }
}
