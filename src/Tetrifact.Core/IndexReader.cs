using BsDiff;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
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

        public IEnumerable<string> GetAllPackageIds(string project)
        {
            string packagesPath = PathHelper.GetExpectedPackagesPath(_settings, project);
            IEnumerable<string> rawList = Directory.GetDirectories(packagesPath);
            return rawList.Select(r => Path.GetRelativePath(packagesPath, r));
        }

        public IEnumerable<string> GetPackageIds(string project, int pageIndex, int pageSize)
        {
            string packagesPath = PathHelper.GetExpectedPackagesPath(_settings, project);
            IEnumerable<string> rawList = Directory.GetDirectories(packagesPath);
            return rawList.Select(r => Path.GetRelativePath(packagesPath, r)).OrderBy(r => r).Skip(pageIndex).Take(pageSize);
        }

        public bool PackageNameInUse(string project, string id)
        {
            string packagesPath = PathHelper.GetExpectedPackagesPath(_settings, project);
            string packagePath = Path.Join(packagesPath, id);
            return Directory.Exists(packagePath);
        }

        public Manifest GetManifest(string project, string packageId)
        {
            string packagesPath = PathHelper.GetExpectedPackagesPath(_settings, project);
            string filePath = Path.Join(packagesPath, packageId, "manifest.json");
            if (!File.Exists(filePath))
                return null;

            try
            {
                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(filePath));
                return manifest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error trying to reading manifest @ {filePath}");
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

        private string RehydrateOrResolve(string project, string package, string filePath) 
        {
            string projectPath = PathHelper.GetExpectedProjectPath(_settings, project);
            string rehydratePatchPath = Path.Combine(projectPath, Constants.ShardsFragment, package, filePath, "patch");
            string rehydrateBinarySourcePath = Path.Combine(projectPath, Constants.ShardsFragment, package, filePath, "bin");
            string rehydrateOutputPath = Path.Combine(this._settings.TempBinaries, project, package, filePath, "bin");

            // if neither patch nor bin exist, file doesn't exist, this will happen in first call
            if (!File.Exists(rehydratePatchPath) && !File.Exists(rehydrateBinarySourcePath))
                return null;

            if (File.Exists(rehydrateBinarySourcePath))
                return rehydrateBinarySourcePath;

            // file has already been rehydrated by a previous process and is ready to serve
            if (File.Exists(rehydrateOutputPath)) 
                return rehydrateOutputPath;

            // check if this package was added to a predecessor, if so, recurse down through all children
            string predecessorPackage = this.GetPredecessor(project, package);
            if (!string.IsNullOrEmpty(predecessorPackage))
                rehydrateBinarySourcePath = RehydrateOrResolve(project, predecessorPackage, filePath);

            using (FileStream rehydrationFileStream = new FileStream(rehydrateOutputPath, FileMode.Create, FileAccess.Write))
            using (FileStream rehydrateSourceStream = new FileStream(rehydrateBinarySourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryPatchUtility.Apply(rehydrateSourceStream, () => { return new FileStream(rehydratePatchPath, FileMode.Open, FileAccess.Read, FileShare.Read); }, rehydrationFileStream);
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

        public void DeletePackage(string project, string packageId)
        {
            string packagesPath = PathHelper.GetExpectedPackagesPath(_settings, project);
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
            string headPath = PathHelper.GetExpectedHeadDirectoryPath(_settings, project);
            List<string> files = Directory.GetFiles(headPath).OrderByDescending(r => r).ToList();
            if (!files.Any())
                return null;

            return FileHelper.GetPackageFromFileName(Path.GetFileNameWithoutExtension(files.First()));
        }


        public string GetPredecessor(string project, string package) 
        {
            string headPath = PathHelper.GetExpectedHeadDirectoryPath(_settings, project);
            List<string> files = Directory.GetFiles(headPath).OrderByDescending(r => r).ToList();

            
            for (int i = 0 ; i < files.Count; i ++ ) 
            {
                // find this package's head update
                string filePackage = FileHelper.GetPackageFromFileName(files[i]);
                if (filePackage != package)
                    continue;

                // reached end of files
                if (i == files.Count - 1)
                    continue;

                return FileHelper.GetPackageFromFileName(files[i + 1]);
            }

            return null;
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
