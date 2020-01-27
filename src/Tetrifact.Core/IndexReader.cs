using BsDiff;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using VCDiff.Decoders;
using VCDiff.Includes;

namespace Tetrifact.Core
{
    public class IndexReader : IIndexReader
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<IIndexReader> _logger;

        #endregion

        #region CTORS

        /// <summary>
        /// Not under IOC control.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        public IndexReader(ISettings settings, ILogger<IIndexReader> logger) 
        {
            _settings = settings;
            _logger = logger;
        }

        #endregion

        #region METHODS

        public bool ProjectExists(string project) 
        {
            return Directory.Exists(Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(project)));
        }

        public DirectoryInfo GetActiveTransactionInfo(string project) 
        {
            return new DirectoryInfo(Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment))
                .GetDirectories().Where(r => !r.Name.StartsWith("~") && !r.Name.StartsWith(PathHelper.DeleteFlag)).OrderByDescending(d => d.Name)
                .FirstOrDefault();
        }

        public IEnumerable<DirectoryInfo> GetRecentTransactionsInfo(string project, int count)
        {
            return new DirectoryInfo(Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment))
                .GetDirectories().Where(r => !r.Name.StartsWith("~") && !r.Name.StartsWith(PathHelper.DeleteFlag)).OrderByDescending(d => d.Name)
                .Take(count);
        }

        private IEnumerable<string> GetManifestPointers(string project) 
        {
            DirectoryInfo latestTransactionInfo = this.GetActiveTransactionInfo(project);
            if (latestTransactionInfo == null)
                return new string[]{ };

            return Directory.GetFiles(latestTransactionInfo.FullName, "*_manifest").Select(r => Path.GetFileName(r));
        }

        public IEnumerable<string> GetManifestPaths(string project) 
        {
            DirectoryInfo latestTransactionInfo = this.GetActiveTransactionInfo(project);
            if (latestTransactionInfo == null)
                return new string[] { };

            IEnumerable<string> pointers = Directory.GetFiles(latestTransactionInfo.FullName, "*_manifest");

            List<string> manifests = new List<string>();
            foreach (string pointer in pointers) 
                manifests.Add(File.ReadAllText(pointer));

            return manifests;
        }

        public bool PackageNameInUse(string project, string id)
        {
            IEnumerable<string> rawList = this.GetManifestPointers(project);
            // remove "_manifest" and decloak, as we want to get package ids from file names
            rawList = rawList.Select(r => Obfuscator.Decloak(Path.GetFileName(r).Replace("_manifest", string.Empty)));

            return rawList.Contains(id);
        }

        public Manifest GetManifest(string project, string packageId)
        {
            DirectoryInfo latestTransactionInfo = this.GetActiveTransactionInfo(project);
            if (latestTransactionInfo == null)
                return null;

            string manifestPointerPath = Path.Combine(latestTransactionInfo.FullName, $"{Obfuscator.Cloak(packageId)}_manifest");
            if (!File.Exists(manifestPointerPath))
                throw new PackageNotFoundException(packageId);

            string manifestRealPath = Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment, File.ReadAllText(manifestPointerPath));

            if (!File.Exists(manifestRealPath))
                return null;

            try
            {
                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestRealPath));
                manifest.PathOnDisk = manifestRealPath;
                return manifest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error trying to reading manifest @ {manifestRealPath}");
                return null;
            }
        }

        public GetFileResponse GetFile(string project, string id)
        {
            FileIdentifier fileIdentifier = FileIdentifier.Decloak(id);

            string binPath = RehydrateOrResolveFile(project, fileIdentifier.Package, fileIdentifier.Path);

            if (string.IsNullOrEmpty(binPath)) 
                throw new Tetrifact.Core.FileNotFoundException(fileIdentifier.Path);
            else
                return new GetFileResponse(new FileStream(binPath, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(fileIdentifier.Path));
        }

        public string RehydrateOrResolveFile(string project, string package, string filePath) 
        {
            string projectPath = PathHelper.GetExpectedProjectPath(_settings, project);
            string shardGuid = PathHelper.GetLatestShardAbsolutePath(this, project, package);
            string dataPathBase = Path.Combine(projectPath, Constants.ShardsFragment, shardGuid, filePath);
            string rehydrateOutputPath = Path.Combine(this._settings.TempBinaries, Obfuscator.Cloak(project), Obfuscator.Cloak(package), filePath, "bin");

            Manifest manifest = this.GetManifest(project, package);
            ManifestItem manifestItem = manifest.Files.FirstOrDefault(r => r.Path == filePath);
            // if neither patch nor bin exist, file doesn't exist
            if (manifestItem == null)
                return null;

            // file has already been rehydrated by a previous process and is ready to serve
            if (File.Exists(rehydrateOutputPath))
                return rehydrateOutputPath;

            FileHelper.EnsureParentDirectoryExists(rehydrateOutputPath);

            for (int i = 0; i < manifestItem.Chunks.Count; i ++)
            {
                ManifestItemChunk chunk = manifestItem.Chunks[i];

                if (chunk.Type == ManifestItemTypes.Bin)
                {
                    using (FileStream writeStream = new FileStream(rehydrateOutputPath, FileMode.OpenOrCreate, FileAccess.Write))
                    using (FileStream readStream = new FileStream(Path.Combine(dataPathBase, $"chunk_{i}"), FileMode.Open, FileAccess.Read))
                    {
                        writeStream.Position = writeStream.Length; // always append to end of this stream
                        StreamsHelper.StreamCopy(readStream, writeStream, readStream.Length);
                    }
                }
                else if (chunk.Type == ManifestItemTypes.Link)
                {
                    // read chunk link from source
                    string binarySourcePath = RehydrateOrResolveFile(project, manifest.DependsOn, filePath);

                    using (FileStream writeStream = new FileStream(rehydrateOutputPath, FileMode.OpenOrCreate, FileAccess.Write))
                    using (FileStream readStream = new FileStream(Path.Combine(binarySourcePath), FileMode.Open, FileAccess.Read))
                    {
                        readStream.Position = i * manifest.FileChunkSize;
                        writeStream.Position = writeStream.Length; // always append to end of this stream
                        StreamsHelper.StreamCopy(readStream, writeStream, (i + 1) * manifest.FileChunkSize);
                    }
                }
                else 
                {
                    // read source chunk against self patch
                    string binarySourcePath = RehydrateOrResolveFile(project, manifest.DependsOn, filePath);

                    using (FileStream writeStream = new FileStream(rehydrateOutputPath, FileMode.OpenOrCreate, FileAccess.Write))
                    using (FileStream binarySourceStream = new FileStream(Path.Combine(binarySourcePath), FileMode.Open, FileAccess.Read))
                    using (FileStream patchStream = new FileStream(Path.Combine(dataPathBase, $"chunk_{i}"), FileMode.Open, FileAccess.Read))
                    using (MemoryStream binarySourceChunkStream = new MemoryStream())
                    {
                        // we want only a portion of the binary source file, so we copy that portion to a chunk memory stream
                        binarySourceStream.Position = i * manifest.FileChunkSize;
                        StreamsHelper.StreamCopy(binarySourceStream, binarySourceChunkStream, ((i + 1) * manifest.FileChunkSize));
                        binarySourceChunkStream.Position = 0;

                        writeStream.Position = writeStream.Length; // always append to end of this stream

                        // if patch is empty, write an empty output file
                        if (patchStream.Length > 0)
                        {
                            VCDecoder decoder = new VCDecoder(binarySourceChunkStream, patchStream, writeStream);

                            // You must call decoder.Start() first. The header of the delta file must be available before calling decoder.Start()

                            VCDiffResult result = decoder.Start();

                            if (result != VCDiffResult.SUCCESS)
                            {
                                //error abort
                                throw new Exception($"vcdiff abort error in file {filePath}");
                            }

                            long bytesWritten = 0;
                            result = decoder.Decode(out bytesWritten);

                            if (result != VCDiffResult.SUCCESS)
                            {
                                //error decoding
                                throw new Exception($"vcdiff decode error in file {filePath}");
                            }
                        }
                    }
                }
            }

            return rehydrateOutputPath;
        }

        public Stream GetPackageAsArchive(string project, string packageId)
        {
            string archivePath = this.GetArchivePath(project, packageId);

            // create
            if (!File.Exists(archivePath))
                this.CreateArchive(project, packageId);

            // is archive still building?
            string tempPath = this.GetTempArchivePath(project, packageId);
            DateTime start = DateTime.Now;
            TimeSpan timeout = new TimeSpan(0, 0, _settings.ArchiveWaitTimeout);

            while (File.Exists(tempPath))
            {
                Thread.Sleep(this._settings.ArchiveAvailablePollInterval);
                if (DateTime.Now - start > timeout)
                    throw new TimeoutException($"Timeout waiting for package ${packageId} archive to build");
            }

            return new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        // todo : make private
        public string GetArchivePath(string project, string packageId)
        {
            return Path.Combine(_settings.ArchivePath, string.Format($"{Obfuscator.Cloak(project)}_{Obfuscator.Cloak(packageId)}.zip"));
        }

        public string GetTempArchivePath(string project, string packageId)
        {
            return Path.Combine(_settings.ArchivePath, string.Format($"{Obfuscator.Cloak(project)}_{Obfuscator.Cloak(packageId)}.zip.tmp"));
        }

        public string GetHead(string project) 
        {
            DirectoryInfo activeTransaction = this.GetActiveTransactionInfo(project);
            if (activeTransaction == null)
                return null;

            string headPath = Path.Combine(activeTransaction.FullName, "head");
            if (!File.Exists(headPath))
                return null;

            return File.ReadAllText(headPath);
        }

        private bool DoesPackageExist(string project, string packageId)
        {
            Manifest manifest = this.GetManifest(project, packageId);
            return manifest != null;
        }

        private void CreateArchive(string project, string packageId)
        {
            // store path with .tmp extension while building, this is used to detect if archiving has already started
            string archivePathTemp = this.GetTempArchivePath(project, packageId);

            // if temp archive exists, it's already building
            if (File.Exists(archivePathTemp))
                return;

            if (!this.DoesPackageExist(project, packageId))
                throw new PackageNotFoundException(packageId);

            // create zip file on disk asap to lock file name off
            using (FileStream zipStream = new FileStream(archivePathTemp, FileMode.Create))
            {
                Manifest manifest = this.GetManifest(project, packageId);

                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in manifest.Files)
                    {
                        ZipArchiveEntry fileEntry = archive.CreateEntry(file.Path);

                        using (Stream entryStream = fileEntry.Open())
                        {
                            using (Stream itemStream = this.GetFile(project, file.Id).Content)
                            {
                                itemStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
            }

            // flip temp file to final path, it is ready for use only when this happens
            string archivePath = this.GetArchivePath(project, packageId);
            File.Move(archivePathTemp, archivePath);
        }

        #endregion
    }
}
