using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.IO.Compression;
using SharpCompress.Readers;
using SharpCompress.Readers.Tar;
using SharpCompress.Common;
using VCDiff.Encoders;
using VCDiff.Includes;
using System.Diagnostics;

namespace Tetrifact.Core
{
    public class PackageCreate : IPackageCreate
    {
        #region FIELDS

        private readonly IIndexReader _indexReader;

        private readonly ILogger<IPackageCreate> _log;

        private string _project;

        private StringBuilder _hashes;

        private IPackageList _packageList;

        #endregion

        #region PROPERTIES

        public string WorkspacePath { get; private set; }

        public Package Package { get; private set; }

        #endregion

        #region CTORS

        public PackageCreate(IIndexReader indexReader, IPackageList packageList, ILogger<IPackageCreate> log)
        {
            _indexReader = indexReader;
            _log = log;
            _packageList = packageList;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifest"></param>
        public PackageCreateResult Create(PackageCreateArguments createArgs)
        {
            try
            {
                // wait until current write process is free
                WriteLock.Instance.WaitUntilClear(createArgs.Project);

                // validate the contents of "newPackage" object
                if (!createArgs.Files.Any())
                    throw new PackageCreateException { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Files collection is empty." };

                if (string.IsNullOrEmpty(createArgs.Id))
                    throw new PackageCreateException { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Id is required." };

                // ensure package does not already exist
                if (_indexReader.PackageNameInUse(createArgs.Project, createArgs.Id))
                    throw new PackageCreateException { ErrorType = PackageCreateErrorTypes.PackageExists };

                // if archive, ensure correct file count
                if (createArgs.IsArchive && createArgs.Files.Count() != 1)
                    throw new PackageCreateException { ErrorType = PackageCreateErrorTypes.InvalidFileCount };

                // if branchFrom package is specified, ensure that package exists (read its manifest as proof)
                if (!string.IsNullOrEmpty(createArgs.Parent) && _indexReader.GetPackage(createArgs.Project, createArgs.Parent) == null)
                    throw new PackageCreateException { ErrorType = PackageCreateErrorTypes.InvalidDiffAgainstPackage };

                this.Initialize(createArgs.Project);

                // if archive, unzip
                if (createArgs.IsArchive) {
                    PackageCreateItem incomingArchive = createArgs.Files.First();
                    
                    // get extension from archive file
                    string extensionRaw = Path.GetExtension(incomingArchive.FileName).Replace(".", string.Empty).ToLower();

                    // if archive, ensure correct file format 
                    if (string.IsNullOrEmpty(extensionRaw))
                        return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat };

                    object archiveTypeTest = null;
                    if (!Enum.TryParse(typeof(ArchiveTypes), extensionRaw, out archiveTypeTest))
                        return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat };
                    ArchiveTypes archiveType = (ArchiveTypes)archiveTypeTest;

                    this.UnpackArchive(incomingArchive.Content, archiveType);
                }
                else
                    foreach (PackageCreateItem formFile in createArgs.Files)
                        this.AddIncomingFile(formFile.Content, formFile.FileName);

                this.StageAllFiles(createArgs.Id, createArgs.Parent);

                this.Package.Description = createArgs.Description;
                this.Package.UniqueId = Guid.NewGuid();

                // we calculate package hash from a sum of all child hashes
                Transaction transaction = new Transaction(_indexReader, createArgs.Project);
                this.Finalize(createArgs.Id, transaction, createArgs.Parent);
                transaction.Commit();
 
                return new PackageCreateResult { Success = true, PackageHash = this.Package.Hash };
            }
            finally
            {
                WriteLock.Instance.Clear(createArgs.Project);
                this.CleanUp();
            }
        }

        public void CreateFromExisting(string project, string packageName, Transaction transaction, string parentPackageName) 
        {
            try
            {
                this.Initialize(project);

                // rehydrate entire package to temp location
                Package package = _indexReader.GetPackage(project, packageName);
                foreach (PackageItem file in package.Files)
                {
                    using (Stream sourceFile = _indexReader.GetFile(project, file.Id).Content)
                    {
                        this.AddIncomingFile(sourceFile, file.Path);
                    }
                }

                this.StageAllFiles(packageName, parentPackageName);
                this.Package.UniqueId = package.UniqueId; // reuse id, package has identical content
                this.Finalize(packageName, transaction, parentPackageName);
            }
            finally 
            {
                this.CleanUp();
            }
        }

        /// <summary>
        /// Note : no catch / log in this method as it is always called from daemon which has its own log
        /// </summary>
        /// <param name="project"></param>
        /// <param name="packageId"></param>
        public void CreateDiffed(string project, string packageId)
        {
            this.Initialize(project);
            Stopwatch stopwatch = Stopwatch.StartNew();

            Package package = _indexReader.GetPackage(project, packageId);
            Package headPackage = string.IsNullOrEmpty(package.Parent) ? null : _indexReader.GetPackage(project, package.Parent);

            if (package.Parent == null)
                throw new Exception("cannot diff a package with no parent");

            foreach (PackageItem manifestItem in package.Files)
            {
                // count how many chunks file should be divided into.
                int chunks = (int)(manifestItem.Size / Settings.FileChunkSize);
                if (manifestItem.Size % Settings.FileChunkSize != 0)
                    chunks++;

                // there should always at least 1 chunk - if the file is zero length, chunks will be zero, force this to 1
                if (chunks == 0)
                    chunks = 1;

                // create patch against head version of file
                string thisFileBinPath = _indexReader.RehydrateOrResolveFile(project, packageId, manifestItem.Path);
                string ancestorBinPath = _indexReader.RehydrateOrResolveFile(project, package.Parent, manifestItem.Path);
                string stagingBasePath = Path.Combine(this.WorkspacePath, Constants.StagingFragment, manifestItem.Path);
                bool linkDirect = headPackage != null && headPackage.Files.Where(r => r.Path == manifestItem.Path).FirstOrDefault()?.Hash == manifestItem.Hash;

                PackageItem newManifestItem = new PackageItem
                {
                    Path = manifestItem.Path,
                    Hash = manifestItem.Hash,
                    Size = manifestItem.Size,
                    Id = manifestItem.Id
                };

                for (int i = 0; i < chunks; i++)
                {
                    // no upstream file to compare against, leave as is
                    if (ancestorBinPath == null) 
                        continue;

                    PackageItemTypes itemType = PackageItemTypes.Bin;

                    string writePath = Path.Combine(stagingBasePath, $"chunk_{i}");

                    if (linkDirect) 
                    {
                        itemType = PackageItemTypes.Link;
                    }
                    else if (new FileInfo(ancestorBinPath).Length < i * Settings.FileChunkSize)
                    {
                        // check if upstream file has content at for this chunk point. if not, write the entire incoming file as a "bin" type
                        StreamsHelper.FileCopy(ancestorBinPath, writePath, i * Settings.FileChunkSize, ((i + 1) * Settings.FileChunkSize));
                    }
                    else
                    {
                        itemType = PackageItemTypes.Patch;

                        FileHelper.EnsureParentDirectoryExists(writePath);

                        // write to patchPath, using incomingFilePath diffed against sourceBinPath
                        using (FileStream patchStream = new FileStream(writePath, FileMode.Create, FileAccess.Write))
                        using (FileStream binarySourceStream = new FileStream(ancestorBinPath, FileMode.Open, FileAccess.Read))
                        using (MemoryStream binarySourceChunkStream = new MemoryStream())
                        {
                            // we want only a portion of the binary source file, so we copy that portion to a chunk memory stream
                            binarySourceStream.Position = i * Settings.FileChunkSize;
                            StreamsHelper.StreamCopy(binarySourceStream, binarySourceChunkStream, ((i + 1) * Settings.FileChunkSize));
                            binarySourceChunkStream.Position = 0;

                            using (FileStream incomingFileStream = new FileStream(thisFileBinPath, FileMode.Open, FileAccess.Read))
                            using (MemoryStream incomingFileChunkStream = new MemoryStream())
                            {
                                // similarly, we want only a portion of the incoming file
                                incomingFileStream.Position = i * Settings.FileChunkSize;
                                StreamsHelper.StreamCopy(incomingFileStream, incomingFileChunkStream, ((i + 1) * Settings.FileChunkSize));
                                incomingFileChunkStream.Position = 0;

                                // if incoming stream is empty, we'll jump over this and end up with an empty patch
                                if (incomingFileChunkStream.Length >= 0)
                                {
                                    VCCoder coder = new VCCoder(binarySourceChunkStream, incomingFileChunkStream, patchStream);
                                    VCDiffResult result = coder.Encode(); //encodes with no checksum and not interleaved
                                    if (result != VCDiffResult.SUCCESS)
                                    {
                                        string error = $"Error patching incoming file {ancestorBinPath} against source {thisFileBinPath}.";
                                        Console.WriteLine(error);
                                        throw new Exception(error);
                                    }
                                }
                            }
                        }
                    }

                    if (itemType != PackageItemTypes.Link)
                        this.Package.SizeOnDisk += new FileInfo(writePath).Length;

                    manifestItem.Chunks.Add(new PackageItemChunk
                    {
                        Id = i,
                        Type = itemType
                    });

                } // for i chunks

                this.Package.Files.Add(newManifestItem);

            } // foreach manifestitem

            this.Package.IsDiffed = true;
            this.Package.FileChunkSize = Settings.FileChunkSize;
            this.Package.Parent = package.Parent;
            this.Package.Name = package.Name;
            this.Package.Size = package.Size;
            this.Package.UniqueId = package.UniqueId; // this has to be carried over, as the package contains the same data.

            // ensure package still exists when publishing
            Package packageCheck = _indexReader.GetPackage(project, packageId);
            if (packageCheck == null || packageCheck.UniqueId != package.UniqueId)
                return;

            // create a transaction for each diffed package instead of grouping them into single transaction, large packages can be costly to process
            // and we want to maximumize the chances of as many getting through as possible
            Transaction transaction = new Transaction(_indexReader, project);
            this.Finalize(this.Package.Name, transaction, this.Package.Parent);
            transaction.Commit();

            // flush package list to update
            _packageList.Clear(project);

            _log.LogInformation($"Packing down package \"{packageId}\" completed, took {stopwatch.ElapsedMilliseconds / 1000} seconds for {this.Package.Size} bytes.");
        }

        /// <summary>
        /// Sets state of this object so it can create a package. This must be called from each method which creates a package, regardless of approach.
        /// </summary>
        /// <param name="project"></param>
        private void Initialize(string project)
        {
            _hashes = new StringBuilder();
            _project = project;
            this.Package = new Package();

            // workspaces have random names, for safety ensure name is not already in use. There's no loop-of-death checking
            // here, but if we cannot generate a true GUID over multiple iterations of this loop, we have bigger problems.
            while (true)
            {
                this.WorkspacePath = Path.Combine(Settings.TempPath, Guid.NewGuid().ToString());
                if (!Directory.Exists(this.WorkspacePath))
                    break;
            }

            // create temp path we'll do all work in
            Directory.CreateDirectory(this.WorkspacePath);

            // inside temp path ...
            // create an incoming folder, this is where uploaded files are placed, where archive files are unpacked if applicable, but also where files from an existing package are assembled to create
            // a new package from.
            Directory.CreateDirectory(Path.Combine(this.WorkspacePath, "incoming"));

            // create staging directory, this is where files are copied from incoming (if applicable), also where patch files are generated before everything is finally moved to a shard folder
            Directory.CreateDirectory(Path.Combine(this.WorkspacePath, Constants.StagingFragment));
        }

        /// <summary>
        /// Writes file from stream to incoming folder - stream can be a form upload, but also from an existing package's file
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        private bool AddIncomingFile(Stream formFile, string relativePath)
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

        /// <summary>
        /// Moves files from incoming
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="parentPackageName"></param>
        private void StageAllFiles(string packageName, string parentPackageName)
        {
            // get all files which were uploaded, sort alphabetically for combined hashing
            string[] files = this.GetIncomingFileNames().ToArray();
            Array.Sort(files, (x, y) => String.Compare(x, y));
            Package parentPackage = string.IsNullOrEmpty(parentPackageName) ? null : _indexReader.GetPackage(_project, parentPackageName);

            foreach (string filePathRaw in files)
            {
                // force unix path 
                string filePath = filePathRaw.Replace("\\", "/");

                // get hash of incoming file
                string fileHash = this.GetIncomingFileHash(filePath);
                _hashes.Append(HashService.FromString(filePath));
                _hashes.Append(fileHash);

                if (string.IsNullOrEmpty(parentPackageName)) 
                {
                    parentPackageName = _indexReader.GetHead(_project);
                    // when deleting the 2nd last package, we can end up with package linking to itself, block this
                    if (parentPackageName == packageName)
                        parentPackageName = null;
                }

                string incomingFilePath = Path.Combine(this.WorkspacePath, "incoming", filePath);
                string stagingBasePath = Path.Combine(this.WorkspacePath, Constants.StagingFragment, filePath); // this is a directory path, but for the literal file path name

                FileHelper.EnsureDirectoryExists(stagingBasePath);

                FileInfo fileInfo = new FileInfo(incomingFilePath);
                this.Package.Size += fileInfo.Length;

                // count how many chunks file should be divided into
                int chunks = (int)(fileInfo.Length / Settings.FileChunkSize);
                if (fileInfo.Length % Settings.FileChunkSize != 0)
                    chunks++;

                // there should always at least 1 chunk - if the file is zero length, chunks will be zero, force this to 1
                if (chunks == 0)
                    chunks = 1;

                // determine file-level (cross-chunk) usages
                string writePath = null;
                bool useFileAsBin = parentPackage == null || !parentPackage.Files.Where(r => r.Path == filePath).Any();
                bool linkDirect = parentPackage != null && parentPackage.Files.Where(r => r.Path == filePath).FirstOrDefault()?.Hash == fileHash;

                PackageItem manifestItem = new PackageItem
                {
                    Path = filePath.Replace("\\", "/"),
                    Hash = fileHash,
                    Size = fileInfo.Length,
                    Id = FileIdentifier.Cloak(packageName, filePath.Replace("\\", "/"))
                };

                for (int i = 0; i < chunks; i++) 
                {
                    
                    writePath = Path.Combine(stagingBasePath, $"chunk_{i}");

                    // treat as bin, ie, copy section directly
                    StreamsHelper.FileCopy(incomingFilePath, writePath, i * Settings.FileChunkSize, ((i + 1) * Settings.FileChunkSize));
                    this.Package.SizeOnDisk += new FileInfo(writePath).Length;

                    manifestItem.Chunks.Add(new PackageItemChunk { 
                        Id = i,
                        Type = PackageItemTypes.Bin
                    });

                } // for chunks

                this.Package.IsDiffed = this.Package.Parent == null ? true : false; // if this package has no ancestors, mark as already diffed, else we'll diff it later. 
                this.Package.FileChunkSize = Settings.FileChunkSize;
                this.Package.Parent = parentPackageName;
                this.Package.Files.Add(manifestItem);
            }
        }

        private void Finalize(string package, Transaction transaction, string parentPackageName)
        {
            string parentPackageNameCheck = parentPackageName;
            if (string.IsNullOrEmpty(parentPackageNameCheck)) 
            {
                parentPackageNameCheck = _indexReader.GetHead(_project);
                // package cannot depend on itself, this will happen when deleting the last package, force null
                if (parentPackageNameCheck == package)
                    parentPackageNameCheck = null;
            }

            // calculate package hash from child hashes: this is the hash of the concatenated hashes of each file's path + each file's contented, sorted by file path.
            this.Package.Name = package;
            this.Package.Hash = HashService.FromString(_hashes.ToString());
            this.Package.Parent = parentPackageNameCheck;

            transaction.AddManifest(this.Package);
            transaction.AddShard(package, Path.Combine(this.WorkspacePath, Constants.StagingFragment));

            if (!string.IsNullOrEmpty(parentPackageNameCheck))
                transaction.AddDependecy(parentPackageNameCheck, package);

            if (string.IsNullOrEmpty(parentPackageName))
                transaction.SetHead(package);
        }

        private IEnumerable<string> GetIncomingFileNames()
        {
            IList<string> rawPaths = Directory.GetFiles(this.WorkspacePath, "*.*", SearchOption.AllDirectories);
            string relativeRoot = Path.Combine(this.WorkspacePath, "incoming");
            return rawPaths.Select(rawPath => Path.GetRelativePath(relativeRoot, rawPath));
        }

        /// <summary>
        /// Unpacks an archive (tar or zip) to incoming folder. This is used only if the packge is uploaded as a single-file archive.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="type"></param>
        private void UnpackArchive(Stream file, ArchiveTypes type)
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

        /// <summary>
        /// Cleans up after creating a package.
        /// </summary>
        private void CleanUp()
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
