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

        private readonly ILogger<IWorkspace> _logger;

        private string _project;

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
        }

        #endregion

        #region METHODS

        public void Initialize(string project)
        {
            this.Manifest = new Manifest();
            this._project = project;

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



            // ensure that project prerequisite folders have been created
            string projectsRoot = Path.Combine(_settings.ProjectsPath, project);
            if (!Directory.Exists(projectsRoot))
                Directory.CreateDirectory(projectsRoot);

            string packagesPath = Path.Combine(projectsRoot, Constants.PackagesFragment);
            if (!Directory.Exists(packagesPath))
                Directory.CreateDirectory(packagesPath);

            string repositoryPath = Path.Combine(projectsRoot, Constants.RepositoryFragment);
            if (!Directory.Exists(repositoryPath))
                Directory.CreateDirectory(repositoryPath);

            string tagsPath = Path.Combine(projectsRoot, Constants.TagsFragment);
            if (!Directory.Exists(tagsPath))
                Directory.CreateDirectory(tagsPath);

            string headPath = Path.Combine(projectsRoot, Constants.HeadFragment);
            if (!Directory.Exists(headPath))
                Directory.CreateDirectory(headPath);
        }

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
            string reposPath = PathHelper.GetExpectedRepositoryPath(_settings, _project);
            string targetPath = Path.Combine(reposPath, filePath, hash, "bin");
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

        public void WriteManifest(string project, string package, string combinedHash)
        {
            // calculate package hash from child hashes
            this.Manifest.Hash = combinedHash;
            string targetFolder = Path.Combine(_settings.ProjectsPath, project, Constants.PackagesFragment, package);
            Directory.CreateDirectory(targetFolder);
            File.WriteAllText(Path.Join(targetFolder, "manifest.json"), JsonConvert.SerializeObject(this.Manifest));
        }


        public void UpdateHead(string project, string package, string diffAgainstPackage) 
        {
            // if this package is diff'ed against current head, it can never become head
            if (!string.IsNullOrEmpty(diffAgainstPackage))
                return;

            // if no head data exists, this package automatically becomes the head
            string headFolder = PathHelper.GetExpectedHeadDirectoryPath(_settings, project);
            string thisHead = Path.Combine(headFolder, $"{DateTime.UtcNow.Ticks}");
            if (!Directory.GetFiles(headFolder).Any())
            {
                File.WriteAllText(thisHead, package);
                return;
            }

            // if reach here, package should be treated as next head
            File.WriteAllText(thisHead, package);
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
