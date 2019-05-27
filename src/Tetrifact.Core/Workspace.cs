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

        private ITetriSettings _settings;

        private ILogger<IWorkspace> _logger;

        #endregion

        #region PROPERTIES

        public string WorkspacePath { get; private set; }

        public Manifest Manifest { get; private set; }

        #endregion

        #region CTORS

        public Workspace(ITetriSettings settings, ILogger<IWorkspace> logger)
        {
            _settings = settings;
            _logger = logger;
            this.Manifest = new Manifest();

            // workspaces have random names, for safety ensure name is not already in use
            while (true)
            {
                this.WorkspacePath = Path.Join(_settings.TempPath, Guid.NewGuid().ToString());
                if (!Directory.Exists(this.WorkspacePath))
                    break;
            }

            // create all basic directories for a functional workspace
            Directory.CreateDirectory(this.WorkspacePath);
            Directory.CreateDirectory(Path.Join(this.WorkspacePath, "incoming"));
        }

        #endregion

        #region METHODS

        public bool AddIncomingFile(Stream formFile, string relativePath)
        {
            if (formFile.Length == 0)
                return false;
            
            string targetPath = Path.Join(this.WorkspacePath, "incoming", relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                formFile.CopyTo(stream);
                return true;
            }
        }

        public void WriteFile(string filePath, string hash, string packageId)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash value is required");

            // move file to public folder
            string targetPath = Path.Combine(_settings.RepositoryPath, filePath, hash, "bin");
            string targetDirectory = Path.GetDirectoryName(targetPath);
            string packagesDirectory = Path.Join(targetDirectory, "packages");

            if (!Directory.Exists(targetDirectory))
            {
                // create both directories if the top one doesn't exist
                Directory.CreateDirectory(targetDirectory);
                Directory.CreateDirectory(packagesDirectory);
            } else if (!Directory.Exists(packagesDirectory))
                // create sub after checking
                Directory.CreateDirectory(packagesDirectory);

            bool onDisk = false;

            if (!File.Exists(targetPath)) { 
                File.Move(
                    Path.Join(this.WorkspacePath, "incoming", filePath),
                    targetPath);

                onDisk = true;
            }

            // write package id under hash, subscribing it to that hash
            File.WriteAllText(Path.Join(packagesDirectory, packageId), string.Empty);

            string pathAndHash = FileIdentifier.Cloak(filePath, hash);
            this.Manifest.Files.Add(new ManifestItem { Path = filePath, Hash = hash, Id = pathAndHash });

            FileInfo fileInfo = new FileInfo(targetPath);
            this.Manifest.Size += fileInfo.Length;
            if (onDisk)
                this.Manifest.SizeOnDisk += fileInfo.Length;
        }

        public void WriteManifest(string packageId, string combinedHash)
        {
            // calculate package hash from child hashes
            this.Manifest.Hash = combinedHash;
            string targetFolder = Path.Join(_settings.PackagePath, packageId);
            Directory.CreateDirectory(targetFolder);
            File.WriteAllText(Path.Join(targetFolder, "manifest.json"), JsonConvert.SerializeObject(this.Manifest));
        }

        public IEnumerable<string> GetIncomingFileNames()
        {
            IList<string> rawPaths = Directory.GetFiles(this.WorkspacePath, "*.*", SearchOption.AllDirectories);
            string relativeRoot = Path.Join(this.WorkspacePath, "incoming");
            return rawPaths.Select(rawPath => Path.GetRelativePath(relativeRoot, rawPath));
        }

        public void AddArchiveContent(Stream file)
        {
            using (var archive = new ZipArchive(file))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry != null)
                    {
                        using (var unzippedEntryStream = entry.Open())
                        {
                            string targetFile = Path.Join(this.WorkspacePath, "incoming", entry.FullName);
                            string targetDirectory = Path.GetDirectoryName(targetFile);
                            if (!Directory.Exists(targetDirectory))
                                Directory.CreateDirectory(targetDirectory);

                            // if .Name is empty it's a directory
                            if (!string.IsNullOrEmpty(entry.Name))
                                entry.ExtractToFile(targetFile);
                        }
                    }
                }
            }
        }

        public string GetIncomingFileHash(string relativePath)
        {
            return HashService.FromFile(Path.Join(this.WorkspacePath, "incoming", relativePath));
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
                _logger.LogWarning($"Failed to delete temp folder {this.WorkspacePath}", ex);
            }
        }

        #endregion
    }
}
