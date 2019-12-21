using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using BsDiff;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO.Compression;
using SharpCompress.Readers;
using SharpCompress.Readers.Tar;
using SharpCompress.Common;

namespace Tetrifact.Core
{
    public class PackageCreate : IPackageCreate
    {
        #region FIELDS

        private readonly IIndexReader _indexReader;

        private readonly ILogger<IPackageCreate> _log;

        private readonly ITetriSettings _settings;

        private string _project;

        private StringBuilder _hashes = new StringBuilder();

        #endregion

        #region PROPERTIES

        public string WorkspacePath { get; private set; }

        public Manifest Manifest { get; private set; }

        #endregion

        #region CTORS

        public PackageCreate(IIndexReader indexReader, ILogger<IPackageCreate> log, ITetriSettings settings)
        {
            _indexReader = indexReader;
            _log = log;
            _settings = settings;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifest"></param>
        public PackageCreateResult CreateWithValidation(PackageCreateArguments newPackage)
        {
            try
            {
                // wait until current write process is free
                WriteLock.Instance.WaitUntilClear(newPackage.Project);

                // validate the contents of "newPackage" object
                if (!newPackage.Files.Any())
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Files collection is empty." };

                if (string.IsNullOrEmpty(newPackage.Id))
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Id is required." };

                // ensure package does not already exist
                if (_indexReader.PackageNameInUse(newPackage.Project, newPackage.Id))
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.PackageExists };

                // if archive, ensure correct file count
                if (newPackage.IsArchive && newPackage.Files.Count() != 1)
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidFileCount };

                // if branchFrom package is specified, ensure that package exists (read its manifest as proof)
                if (!string.IsNullOrEmpty(newPackage.BranchFrom) && _indexReader.GetManifest(newPackage.Project, newPackage.BranchFrom) == null) 
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidDiffAgainstPackage };

                this.Initialize(newPackage.Project);

                // if archive, unzip
                if (newPackage.IsArchive) {
                    IFormFile incomingArchive = newPackage.Files.First();
                    Stream archiveFile = incomingArchive.OpenReadStream();
                    
                    // get extension from archive file
                    string extensionRaw = Path.GetExtension(incomingArchive.FileName).Replace(".", string.Empty).ToLower();

                    // if archive, ensure correct file format 
                    if (string.IsNullOrEmpty(extensionRaw))
                        return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat };

                    object archiveTypeTest = null;
                    if (!Enum.TryParse(typeof(ArchiveTypes), extensionRaw, out archiveTypeTest))
                        return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat };
                    ArchiveTypes archiveType = (ArchiveTypes)archiveTypeTest;

                    this.AddArchive(archiveFile, archiveType);
                }
                else
                    foreach (IFormFile formFile in newPackage.Files)
                        this.AddFile(formFile.OpenReadStream(), formFile.FileName);

                this.StageAllFiles(newPackage.Id, newPackage.BranchFrom);

                this.Manifest.Description = newPackage.Description;

                // we calculate package hash from a sum of all child hashes
                Transaction transaction = new Transaction(_settings, _indexReader, newPackage.Project);
                this.Finalize(newPackage.Project, newPackage.Id, newPackage.BranchFrom, transaction);
                transaction.Commit();
 
                return new PackageCreateResult { Success = true, PackageHash = this.Manifest.Hash };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                Console.WriteLine($"Unexpected error : {ex}");
                return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.UnexpectedError };
            }
            finally
            {
                WriteLock.Instance.Clear(newPackage.Project);
                this.Dispose();
            }
        }

        public void CreateFromExisting(string project, string package, string referencePackage, Transaction transaction) 
        {
            try
            {
                this.Initialize(project);

                // rehydrate entire package to temp location
                Manifest manifest = _indexReader.GetManifest(project, package);
                foreach (ManifestItem file in manifest.Files)
                {
                    using (Stream sourceFile = _indexReader.GetFile(project, file.Id).Content)
                    {
                        this.AddFile(sourceFile, file.Path);
                    }
                }

                this.StageAllFiles(package, referencePackage);
                this.Finalize(project, package, referencePackage, transaction);
            }
            finally 
            {
                this.Dispose();
            }
        }

        private void Initialize(string project)
        {
            this.Manifest = new Manifest();
            this._project = project;

            // workspaces have random names, for safety ensure name is not already in use. There's no loop-of-death checking
            // here, but if we cannot generate a true GUID we have bigger problems.
            while (true)
            {
                this.WorkspacePath = Path.Combine(_settings.TempPath, Guid.NewGuid().ToString());
                if (!Directory.Exists(this.WorkspacePath))
                    break;
            }

            // create all directories needed for a functional workspace
            Directory.CreateDirectory(this.WorkspacePath);

            // incoming is where uploaded files first land. If upload is an archive, this is where archive is unpacked to
            Directory.CreateDirectory(Path.Combine(this.WorkspacePath, "incoming"));

            // staying is the next place files are moved to. Staging will contain either the raw file, or a patch of the file vs the version from a previous version 
            Directory.CreateDirectory(Path.Combine(this.WorkspacePath, Constants.StagingFragment));
        }

        private bool AddFile(Stream formFile, string relativePath)
        {
            if (formFile.Length == 0)
                return false;

            string targetPath = Path.Combine(this.WorkspacePath, "incoming", relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                formFile.CopyTo(stream);
                return true;
            }
        }

        private void StageAllFiles(string packageId, string diffAgainstPackage)
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

                string incomingFilePath = Path.Combine(this.WorkspacePath, "incoming", filePath);
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

                    using (FileStream patchOutStream = new FileStream(patchFilePath, FileMode.Create, FileAccess.Write))
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

        private void Finalize(string project, string package, string diffAgainstPackage, Transaction transaction)
        {
            string dependsOn = diffAgainstPackage;
            if (string.IsNullOrEmpty(dependsOn))
                dependsOn = _indexReader.GetHead(_project);

            // calculate package hash from child hashes: this is the hash of the concatenated hashes of each file's path + each file's contented, sorted by file path.
            this.Manifest.Id = package;
            this.Manifest.Hash = HashService.FromString(_hashes.ToString());
            this.Manifest.DependsOn = dependsOn;

            string packageNoCollideName = $"{Guid.NewGuid()}__{Obfuscator.Cloak(package)}";
            string manifestPath = Path.Combine(Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment), packageNoCollideName);
            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(this.Manifest));

            // Move the staging directory to the "shards" folder
            string stagingRoot = Path.Combine(this.WorkspacePath, Constants.StagingFragment);
            string shardRoot = PathHelper.ResolveShardRoot(_settings, _project);
            string finalRoot = Path.Combine(shardRoot, packageNoCollideName);
            FileHelper.MoveDirectoryContents(stagingRoot, finalRoot);

            transaction.AddManifestPointer(package, packageNoCollideName);

            // shard pointer
            transaction.AddShardPointer(package, packageNoCollideName);

            if (!string.IsNullOrEmpty(dependsOn))
                transaction.AddDependecy(dependsOn, package, !string.IsNullOrEmpty(diffAgainstPackage));

            if (string.IsNullOrEmpty(diffAgainstPackage))
                transaction.SetHead(package);
        }

        private IEnumerable<string> GetIncomingFileNames()
        {
            IList<string> rawPaths = Directory.GetFiles(this.WorkspacePath, "*.*", SearchOption.AllDirectories);
            string relativeRoot = Path.Combine(this.WorkspacePath, "incoming");
            return rawPaths.Select(rawPath => Path.GetRelativePath(relativeRoot, rawPath));
        }

        private void AddArchive(Stream file, ArchiveTypes type)
        {
            if (type == ArchiveTypes.zip)
                using (var archive = new ZipArchive(file))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry != null)
                        {
                            using (Stream unzippedEntryStream = entry.Open())
                            {
                                string targetFile = Path.Combine(this.WorkspacePath, "incoming", entry.FullName);
                                string targetDirectory = Path.GetDirectoryName(targetFile);
                                FileHelper.EnsureDirectoryExists(targetDirectory);

                                // if .Name is empty it's a directory
                                if (!string.IsNullOrEmpty(entry.Name))
                                    entry.ExtractToFile(targetFile);
                            }
                        }
                    }
                }


            if (type == ArchiveTypes.gz)
                using (IReader reader = TarReader.Open(file))
                {
                    while (reader.MoveToNextEntry())
                    {
                        IEntry entry = reader.Entry;
                        if (reader.Entry.IsDirectory)
                            continue;

                        using (EntryStream entryStream = reader.OpenEntryStream())
                        {
                            string targetFile = Path.Combine(this.WorkspacePath, "incoming", reader.Entry.Key);
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

        private string GetIncomingFileHash(string relativePath)
        {
            return HashService.FromFile(Path.Combine(this.WorkspacePath, "incoming", relativePath));
        }

        private void Dispose()
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
