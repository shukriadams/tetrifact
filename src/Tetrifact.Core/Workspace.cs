using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BsDiff;
using System.Text;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Readers.Tar;

namespace Tetrifact.Core
{
    public class Workspace : IWorkspace
    {
        #region FIELDS

        private readonly ITetriSettings _settings;

        private readonly ILogger<IWorkspace> _logger;

        private readonly IIndexReader _indexReader;

        private string _project;

        private StringBuilder _hashes = new StringBuilder();

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

        public void StageAllFiles(string packageId, string diffAgainstPackage)
        {
            // get all files which were uploaded, sort alphabetically for combined hashing
            string[] files = this.GetIncomingFileNames().ToArray();
            Array.Sort(files, (x, y) => String.Compare(x, y));

            foreach (string filePath in files)
            {
                // get hash of incoming file
                string fileHash = this.GetIncomingFileHash(filePath);
                _hashes.Append(HashService.FromString(filePath));
                _hashes.Append(fileHash);

                if (string.IsNullOrEmpty(diffAgainstPackage))
                    diffAgainstPackage = _indexReader.GetHead(_project);

                string incomingFilePath = Path.Join(this.WorkspacePath, "incoming", filePath);
                string stagingBasePath = Path.Combine(this.WorkspacePath, Constants.StagingFragment, filePath); // this is a directory path, but for the literal file path name

                FileHelper.EnsureDirectoryExists(stagingBasePath);

                FileInfo patchFileInfo = null;
                Manifest headManifest = string.IsNullOrEmpty(diffAgainstPackage) ? null : _indexReader.GetManifest(_project, diffAgainstPackage);
                

                // if no head, or head doesn't contain the same file path, write incoming as raw bin
                if (headManifest == null || !headManifest.Files.Where(r => r.Path == filePath).Any())
                {
                    string writePath = Path.Combine(stagingBasePath, "bin");
                    File.Copy(incomingFilePath, writePath);

                    patchFileInfo = new FileInfo(writePath);
                }
                else
                {
                    // create patch against head version of file
                    string sourceBinPath = _indexReader.RehydrateOrResolveFile(_project, diffAgainstPackage, filePath);
                    byte[] sourceVersionBinary = File.ReadAllBytes(sourceBinPath); // this is going to hurt on large files, but can't be avoided, bsdiff requires entire file in-memory
                    byte[] incomingVersionBinary = File.ReadAllBytes(incomingFilePath);
                    string patchFilePath = Path.Combine(stagingBasePath, "patch");
                    
                    using(FileStream patchOutStream = new FileStream(patchFilePath, FileMode.Create, FileAccess.Write))
                    {
                        BinaryPatchUtility.Create(sourceVersionBinary, incomingVersionBinary, patchOutStream);
                    }

                    patchFileInfo = new FileInfo(patchFilePath);
                }
                
                this.Manifest.DependsOn = diffAgainstPackage;

                string pathAndHash = FileIdentifier.Cloak(packageId, filePath);
                this.Manifest.Files.Add(new ManifestItem { Path = filePath, Hash = fileHash, Id = pathAndHash });

                FileInfo fileInfo = new FileInfo(incomingFilePath);
                this.Manifest.Size += fileInfo.Length;

                if (patchFileInfo != null)
                    this.Manifest.SizeOnDisk += patchFileInfo.Length;
            }
        }

        public void Commit(string project, string package, string diffAgainstPackage, Transaction transaction)
        {
            string dependsOn = diffAgainstPackage;
            if (string.IsNullOrEmpty(dependsOn))
                dependsOn = _indexReader.GetHead(_project);

            // calculate package hash from child hashes: this is the hash of the concatenated hashes of each file's path + each file's contented, sorted by file path.
            this.Manifest.Id = package;
            this.Manifest.Hash = HashService.FromString(_hashes.ToString());
            this.Manifest.DependsOn = dependsOn;

            string packageNoCollideName = $"{Guid.NewGuid()}__{package}";
            string manifestPath = Path.Join(Path.Combine(_settings.ProjectsPath, project, Constants.ManifestsFragment), packageNoCollideName);
            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(this.Manifest));

            // Move the staging directory to the "shards" folder
            string stagingRoot = Path.Combine(this.WorkspacePath, Constants.StagingFragment);
            string shardRoot = PathHelper.ResolveShardRoot(_settings, _project);
            string finalRoot = Path.Combine(shardRoot, packageNoCollideName);
            FileHelper.MoveDirectoryContents(stagingRoot, finalRoot);

            // we want to commit the transaction locally if it wasn't passed in. If passed in, committing must be done by code
            // that passed transaction in
            bool commitTransaction = transaction == null;

            if (transaction == null) 
                transaction = new Transaction(_settings, _indexReader, project);

            transaction.AddManifestPointer(package, packageNoCollideName);

            // shard pointer
            transaction.AddShardPointer(package, packageNoCollideName);

            if (!string.IsNullOrEmpty(dependsOn))
                transaction.AddDependecy(dependsOn, package, !string.IsNullOrEmpty(diffAgainstPackage));

            if (string.IsNullOrEmpty(diffAgainstPackage))
                transaction.SetHead(package);

            // flip transaction live
            if (commitTransaction)
                transaction.Commit();
        }

        public IEnumerable<string> GetIncomingFileNames()
        {
            IList<string> rawPaths = Directory.GetFiles(this.WorkspacePath, "*.*", SearchOption.AllDirectories);
            string relativeRoot = Path.Join(this.WorkspacePath, "incoming");
            return rawPaths.Select(rawPath => Path.GetRelativePath(relativeRoot, rawPath));
        }

        public void AddTarContent(Stream file) 
        {
            using (IReader reader = TarReader.Open(file))
            {
                while (reader.MoveToNextEntry())
                {
                    IEntry entry = reader.Entry;
                    if (reader.Entry.IsDirectory)
                        continue;

                    using (EntryStream entryStream = reader.OpenEntryStream())
                    {
                        string targetFile = Path.Join(this.WorkspacePath, "incoming", reader.Entry.Key);
                        string targetDirectory = Path.GetDirectoryName(targetFile);
                        if (!Directory.Exists(targetDirectory))
                            Directory.CreateDirectory(targetDirectory);

                        // if .Name is empty it's a directory
                        if (!reader.Entry.IsDirectory)
                            using (var fileStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write))
                            {
                                entryStream.CopyTo(fileStream);
                            }
                    }
                }
            }
        }

        public void AddZipContent(Stream file)
        {
            using (var archive = new ZipArchive(file))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry != null)
                    {
                        using (Stream unzippedEntryStream = entry.Open())
                        {
                            string targetFile = Path.Join(this.WorkspacePath, "incoming", entry.FullName);
                            string targetDirectory = Path.GetDirectoryName(targetFile);
                            FileHelper.EnsureDirectoryExists(targetDirectory);

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
    