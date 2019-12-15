using BsDiff;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Tetrifact.Core
{
    public class IndexReader : IIndexReader
    {
        #region FIELDS

        private readonly ITetriSettings _settings;

        private readonly ILogger<IIndexReader> _logger;

        #endregion

        #region CTORS

        /// <summary>
        /// Not under IOC control.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        public IndexReader(ITetriSettings settings, ILogger<IIndexReader> logger) 
        {
            _settings = settings;
            _logger = logger;
            
        }

        #endregion

        #region METHODS

        public DirectoryInfo GetActiveTransactionInfo(string project) 
        {
            return new DirectoryInfo(Path.Combine(_settings.ProjectsPath, project, Constants.TransactionsFragment))
                .GetDirectories().Where(r => !r.Name.StartsWith("~")).OrderByDescending(d => d.LastWriteTimeUtc)
                .FirstOrDefault();
        }

        public IEnumerable<DirectoryInfo> GetLatestTransactionsInfo(string project, int count)
        {
            return new DirectoryInfo(Path.Combine(_settings.ProjectsPath, project, Constants.TransactionsFragment))
                .GetDirectories().Where(r => !r.Name.StartsWith("~")).OrderByDescending(d => d.LastWriteTimeUtc)
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

        public IEnumerable<string> GetAllPackageIds(string project)
        {
            IEnumerable<string> rawList = this.GetManifestPointers(project);

            // clip "_manifest" from end
            return rawList.Select(r => StringHelper.ClipFromEnd(Path.GetFileName(r), 9));
        }

        public IEnumerable<string> GetPackageIds(string project, int pageIndex, int pageSize)
        {
            IEnumerable<string> rawList = this.GetAllPackageIds(project);
            return rawList.OrderBy(r => r).Skip(pageIndex).Take(pageSize);
        }

        public bool PackageNameInUse(string project, string id)
        {
            IEnumerable<string> rawList = this.GetAllPackageIds(project);
            return rawList.Contains(id);
        }

        public Manifest GetManifest(string project, string packageId)
        {
            DirectoryInfo latestTransactionInfo = this.GetActiveTransactionInfo(project);
            if (latestTransactionInfo == null)
                return null;

            string manifestPointerPath = Path.Combine(latestTransactionInfo.FullName, $"{packageId}_manifest");
            if (!File.Exists(manifestPointerPath))
                throw new PackageNotFoundException(packageId);

            string manifestRealPath = Path.Combine(_settings.ProjectsPath, project, Constants.ManifestsFragment, File.ReadAllText(manifestPointerPath));


            string packagesPath = PathHelper.GetExpectedManifestsPath(_settings, project);
            if (!File.Exists(manifestRealPath))
                return null;

            try
            {
                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestRealPath));
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

            string binPath = RehydrateOrResolve(project, fileIdentifier.Package, fileIdentifier.Path);

            if (string.IsNullOrEmpty(binPath)) 
                throw new Tetrifact.Core.FileNotFoundException(fileIdentifier.Path);
            else
                return new GetFileResponse(new FileStream(binPath, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(fileIdentifier.Path));
        }

        public string RehydrateOrResolve(string project, string package, string filePath) 
        {
            string projectPath = PathHelper.GetExpectedProjectPath(_settings, project);
            string shardGuid = PathHelper.GetLatestShardPath(this, project, package);

            string patchPath = Path.Combine(projectPath, Constants.ShardsFragment, shardGuid, filePath, "patch");
            string binaryPath = Path.Combine(projectPath, Constants.ShardsFragment, shardGuid, filePath, "bin");
            string rehydrateOutputPath = Path.Combine(this._settings.TempBinaries, project, package, filePath, "bin");

            // if neither patch nor bin exist, file doesn't exist, this will happen in first call
            if (!File.Exists(patchPath) && !File.Exists(binaryPath))
                return null;

            // if the binary path already exists, we can use that directly
            if (File.Exists(binaryPath))
                return binaryPath;

            // file has already been rehydrated by a previous process and is ready to serve
            if (File.Exists(rehydrateOutputPath))
                return rehydrateOutputPath;

            // check if this package was added to a predecessor, if so, recurse down through all children
            Manifest manifest = this.GetManifest(project, package);
            if (string.IsNullOrEmpty(manifest.Predecessor))
                throw new Exception($"manifest for package {package} does not have an expected predecessor value");

            // recurse - this ensures that the entire stack of files requireq for patching out is processed
            binaryPath = RehydrateOrResolve(project, manifest.Predecessor, filePath);

            FileHelper.EnsureFileDirectoryExists(rehydrateOutputPath);

            using (FileStream rehydrationFileStream = new FileStream(rehydrateOutputPath, FileMode.Create, FileAccess.Write))
            using (FileStream rehydrateSourceStream = new FileStream(binaryPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryPatchUtility.Apply(rehydrateSourceStream, () => { return new FileStream(patchPath, FileMode.Open, FileAccess.Read, FileShare.Read); }, rehydrationFileStream);
            }

            return rehydrateOutputPath;
        }

        /// <summary>
        /// Todo : this is far too simplistic, expand to delete based on available disk space.
        /// </summary>
        public void PurgeOldArchives()
        {
            DirectoryInfo info = new DirectoryInfo(_settings.ArchivePath);

            IEnumerable<FileInfo> files = info.GetFiles().OrderByDescending(p => p.CreationTime).Skip(_settings.MaxArchives);

            foreach (FileInfo file in files)
            {
                try
                {
                    File.Delete(file.FullName);
                }
                catch (IOException ex)
                {
                    // ignore these, file might be in use, in which case we'll try to delete it next purge
                    _logger.LogWarning($"Failed to purge archive ${file}, assuming in use. Will attempt delete on next pass. ${ex}");
                }
            }
        }

        public Stream GetPackageAsArchive(string project, string packageId)
        {
            string archivePath = this.GetPackageArchivePath(project, packageId);

            // create
            if (!File.Exists(archivePath))
                this.CreateArchive(project, packageId);

            // is archive still building?
            string tempPath = this.GetPackageArchiveTempPath(project, packageId);
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

        public int GetPackageArchiveStatus(string project, string packageId)
        {
            if (!this.DoesPackageExist(project, packageId))
                throw new PackageNotFoundException(packageId);

            string archivePath = this.GetPackageArchivePath(project, packageId);
            string temptPath = this.GetPackageArchiveTempPath(project, packageId);

            // archive doesn't exist and isn't being created
            if (!File.Exists(archivePath) && !File.Exists(temptPath))
                return 0;

            if (File.Exists(temptPath))
                return 1;

            return 2;
        }

        public async void MarkPackageForDelete(string project, string packageId)
        {
            LockRequest lockRequest = new LockRequest();
            try
            {
                await lockRequest.Get();

                Transaction transaction = new Transaction(_settings, this, project);
                transaction.RemoveManifestPointer(packageId);
                transaction.RemoveShardPointer(packageId);
                transaction.Commit();
            }
            finally {
                LinkLock.Instance.Release();
            }
            
            /*
            string packagesPath = PathHelper.GetExpectedManifestsPath(_settings, project);
            string projectPath = PathHelper.GetExpectedProjectPath(_settings, project);

            Manifest manifest = this.GetManifest(project, packageId);
            if (manifest == null)
                throw new PackageNotFoundException(packageId);

            // delete repo entries for this package, the binary will be removed by a cleanup job
            string reposPath = PathHelper.GetExpectedRepositoryPath(_settings, project);
            foreach (ManifestItem item in manifest.Files)
            {
                string targetPath = Path.Combine(reposPath, item.Path, item.Hash, "packages", packageId);
                if (File.Exists(targetPath))
                    File.Delete(targetPath);
            }

            // delete package folder
            string packageFolder = Path.Combine(packagesPath, packageId);
            if (Directory.Exists(packageFolder))
                Directory.Delete(packageFolder, true);

            // delete tag links for package
            string tagsPath = Path.Combine(projectPath, Constants.TagsFragment);
            if (Directory.Exists(tagsPath)) 
            {
                string[] tagFiles = Directory.GetFiles(tagsPath, packageId, SearchOption.AllDirectories);
                foreach (string tagFile in tagFiles)
                {
                    try
                    {
                        File.Delete(tagFile);
                    }
                    catch (IOException ex)
                    {
                        // ignore these, file is being downloaded, it will eventually be nuked by routine cleanup
                        _logger.LogWarning($"Failed to delete tag ${tagFile}, assuming in use. Will attempt delete on next pass. ${ex}");
                    }
                }
            }
            */
        }

        public string GetPackageArchivePath(string project, string packageId)
        {
            return Path.Combine(_settings.ArchivePath, string.Format($"{project}_{packageId}.zip"));
        }

        public string GetPackageArchiveTempPath(string project, string packageId)
        {
            return Path.Combine(_settings.ArchivePath, string.Format($"{project}_{packageId}.zip.tmp"));
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

        public IEnumerable<string> GetProjects() 
        {
            string[] directories = Directory.GetDirectories(_settings.ProjectsPath);
            return directories.Select(r => Path.GetFileName(r));
        }

        private bool DoesPackageExist(string project, string packageId)
        {
            Manifest manifest = this.GetManifest(project, packageId);
            return manifest != null;
        }

        private void CreateArchive(string project, string packageId)
        {
            // store path with .tmp extension while building, this is used to detect if archiving has already started
            string archivePath = this.GetPackageArchivePath(project, packageId);
            string archivePathTemp = this.GetPackageArchiveTempPath(project, packageId);

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
            File.Move(archivePathTemp, archivePath);
        }

        #endregion
    }
}
