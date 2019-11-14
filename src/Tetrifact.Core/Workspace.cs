using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BsDiff;

namespace Tetrifact.Core
{
    public class Workspace : IWorkspace
    {
        #region FIELDS

        private readonly ITetriSettings _settings;

        private readonly ILogger<IWorkspace> _logger;

        private readonly IIndexReader _indexReader;

        private string _project;

        #endregion

        #region PROPERTIES

        public string WorkspacePath { get; private set; }

        public Manifest Manifest { get; private set; }

        #endregion

        #region CTORS

        public Workspace(IIndexReader indexReader, ITetriSettings settings, ILogger<IWorkspace> logger)
        {
            _settings = settings;
            _logger = logger;
            _indexReader = indexReader;
        }

        #endregion

        #region METHODS

        public void Initialize(string project)
        {
            this.Manifest = new Manifest();
            this._project = project;

            // workspaces have random names, for safety ensure name is not already in use. There's no loop-of-death checking
            // here, but if we cannot generate a true GUID we have bigger problems.
            while (true)
            {
                this.WorkspacePath = Path.Join(_settings.TempPath, Guid.NewGuid().ToString());
                if (!Directory.Exists(this.WorkspacePath))
                    break;
            }


            // create all directories needed for a functional workspace
            Directory.CreateDirectory(this.WorkspacePath);
            // incoming is where uploaded files first land. If upload is an archive, this is where archive is unpacked to
            Directory.CreateDirectory(Path.Join(this.WorkspacePath, "incoming"));

            // staying is the next place files are moved to. Staging will contain either the raw file, or a patch of the file vs the version from a previous version 
            Directory.CreateDirectory(Path.Join(this.WorkspacePath, Constants.StagingFragment));

            // ensure that project prerequisite folders have been created. This could was orphaned from IndexReader and ended up here,
            // but it should ideally move into a more generic place.
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

        public void AddIncomingArchive(Stream file)
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

        public void StageFile(string filePath, string hash, string packageId)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash value is required");

            string reposPath = PathHelper.GetExpectedRepositoryPath(_settings, _project);
            string targetPath = Path.Combine(reposPath, filePath, hash, "bin");
            string targetDirectory = Path.GetDirectoryName(targetPath);
            string packagesDirectory = Path.Join(targetDirectory, "packages");

            // create necessary directories in repository to contain file
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
                Directory.CreateDirectory(packagesDirectory);
            }
            else if (!Directory.Exists(packagesDirectory)) 
            {
                Directory.CreateDirectory(packagesDirectory);
            }

            bool onDisk = false;

            string head = _indexReader.GetHead(_project);
            string incomingFilePath = Path.Join(this.WorkspacePath, "incoming", filePath);
            string stagingBasePath = Path.Combine(this.WorkspacePath, Constants.StagingFragment, filePath); // this is a directory path, but for the literal file path name

            if (!Directory.Exists(stagingBasePath))
                Directory.CreateDirectory(stagingBasePath);

            // if no head, or head doesn't contain the same file path, write incoming as raw bin
            string sourceBinPath = PathHelper.ResolveFinalFileBinPath(_settings, _project, packageId, filePath);
            bool writeRaw = string.IsNullOrEmpty(head) || !File.Exists(sourceBinPath);

            if (writeRaw)
            {
                File.Copy(incomingFilePath,
                    Path.Combine(stagingBasePath, "bin"));
            }
            else
            {
                // create patch against head version of file
                byte[] sourceVersionBinary = File.ReadAllBytes(sourceBinPath); // this is going to hurt on large files, but can't be avoided, bsdiff requires entire file in-memory
                byte[] incomingVersionBinary = File.ReadAllBytes(incomingFilePath);
                using (FileStream patchOutStream = new FileStream(Path.Combine(stagingBasePath, "patch"), FileMode.Create, FileAccess.Write))
                {
                    BinaryPatchUtility.Create(sourceVersionBinary, incomingVersionBinary, patchOutStream);
                }

                /*
                REFERENCE CODE FOR APPLYING PATCH, USE, DONT JUST DELETE
                string finalfinalpath = Path.Combine(Path.GetDirectoryName(Path.Combine(this.WorkspacePath, "incoming", filePath)), "FINAL");
                using (FileStream theFinalResult = new FileStream(finalfinalpath, FileMode.Create, FileAccess.Write))
                using (FileStream thehead = new FileStream(headFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryPatchUtility.Apply(thehead, () => { return new FileStream(patchPath, FileMode.Open, FileAccess.Read, FileShare.Read); }, theFinalResult);
                }
                */
            }

            string pathAndHash = FileIdentifier.Cloak(filePath, hash);
            this.Manifest.Files.Add(new ManifestItem { Path = filePath, Hash = hash, Id = pathAndHash });

            FileInfo fileInfo = new FileInfo(incomingFilePath);
            this.Manifest.Size += fileInfo.Length;
            if (onDisk)
                this.Manifest.SizeOnDisk += fileInfo.Length;
        }

        public void Finalize(string project, string package, string combinedHash)
        {
            // calculate package hash from child hashes
            this.Manifest.Hash = combinedHash;
            string targetFolder = Path.Combine(_settings.ProjectsPath, project, Constants.PackagesFragment, package);
            Directory.CreateDirectory(targetFolder);
            File.WriteAllText(Path.Join(targetFolder, "manifest.json"), JsonConvert.SerializeObject(this.Manifest));

            // Move the staging directory to the "shards" folder, once this is done the package is live and visible and cannot be auto rolled back.
            // This is how we "do atomic" in Tetrifact.
            string stagingRoot = Path.Combine(this.WorkspacePath, Constants.StagingFragment);
            string shardRoot = PathHelper.ResolveShardRoot(_settings, _project);
            
            Directory.Move(stagingRoot, shardRoot);
        }

        public void UpdateHead(string project, string package, string diffAgainstPackage) 
        {
            // if this package is diff'ed against current head, it can never become head
            if (!string.IsNullOrEmpty(diffAgainstPackage))
                return;

            // if no head data exists, this package automatically becomes the head
            string headFolder = PathHelper.GetExpectedHeadDirectoryPath(_settings, project);
            string thisHead = Path.Combine(headFolder, $"{DateTime.UtcNow.Ticks}_{package}");
            if (!Directory.GetFiles(headFolder).Any())
            {
                File.WriteAllText(thisHead, package);
                return;
            }

            // if reach here, package should be treated as next head
            File.WriteAllText(thisHead, string.Empty);
        }

        public IEnumerable<string> GetIncomingFileNames()
        {
            IList<string> rawPaths = Directory.GetFiles(this.WorkspacePath, "*.*", SearchOption.AllDirectories);
            string relativeRoot = Path.Join(this.WorkspacePath, "incoming");
            return rawPaths.Select(rawPath => Path.GetRelativePath(relativeRoot, rawPath));
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
