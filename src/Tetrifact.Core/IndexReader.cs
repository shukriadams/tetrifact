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

        private readonly ILogger<IIndexReader> _logger;

        private readonly ITypeProvider _typeProvider;

        #endregion

        #region CTORS

        /// <summary>
        /// Not under IOC control.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        public IndexReader(ILogger<IIndexReader> logger, ITypeProvider typeProvider) 
        {
            _logger = logger;
            _typeProvider = typeProvider;
        }

        #endregion

        #region METHODS

        public Package GetChild(string project, string package) 
        {
            ActiveTransaction activeTransaction = this.GetActiveTransaction(project);

            try
            {
                if (activeTransaction == null)
                    return null;

                string childLink = Directory.GetFiles(activeTransaction.Info.FullName, $"dep_{Obfuscator.Cloak(package)}_*").FirstOrDefault();
                if (childLink == null)
                    return null;

                string childName = Path.GetFileNameWithoutExtension(childLink).Replace($"dep_{Obfuscator.Cloak(package)}_", string.Empty).Replace("_", string.Empty);
                childName = Obfuscator.Decloak(childName);
                return this.GetPackage(project, childName);
            }
            finally 
            {
                if (activeTransaction!= null)
                    activeTransaction.Unlock();
            }

        }


        public bool ProjectExists(string project) 
        {
            return Directory.Exists(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project)));
        }


        public ActiveTransaction GetActiveTransaction(string project) 
        {
            while (true) 
            {
                DirectoryInfo info = new DirectoryInfo(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment))
                    .GetDirectories()
                    .Where(r => !r.Name.StartsWith(Constants.UnpublishedFlag) && !r.Name.StartsWith(PathHelper.DeleteFlag))
                    .OrderByDescending(d => d.Name)
                    .FirstOrDefault();

                if (info == null)
                    return null;

                if (Directory.Exists(info.FullName)) 
                {
                    try
                    {
                        return new ActiveTransaction(info);
                    }
                    catch (MissingTransacationException) 
                    {
                        // ignore these, transaction was deleted during seek
                    }
                }
                    
            }
        }


        public IEnumerable<DirectoryInfo> GetRecentTransactionsInfo(string project, int count)
        {
            return new DirectoryInfo(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment))
                .GetDirectories()
                .Where(r => !r.Name.StartsWith(Constants.UnpublishedFlag) && !r.Name.StartsWith(PathHelper.DeleteFlag))
                .OrderByDescending(d => d.Name)
                .Take(count);
        }


        private IEnumerable<string> GetManifestPointers(string project) 
        {
            ActiveTransaction latestTransactionInfo = this.GetActiveTransaction(project);
            try
            {
                if (latestTransactionInfo == null)
                    return new string[] { };

                return Directory.GetFiles(latestTransactionInfo.Info.FullName, "*_manifest").Select(r => Path.GetFileName(r));
            }
            finally 
            {
                if (latestTransactionInfo != null)
                    latestTransactionInfo.Unlock();
            }


        }


        public IEnumerable<string> GetPackagePaths(string project) 
        {
            ActiveTransaction latestTransactionInfo = this.GetActiveTransaction(project);

            try
            {
                if (latestTransactionInfo == null)
                    return new string[] { };

                IEnumerable<string> pointers = Directory.GetFiles(latestTransactionInfo.Info.FullName, "*_manifest");

                List<string> manifests = new List<string>();
                foreach (string pointer in pointers)
                    manifests.Add(File.ReadAllText(pointer));

                return manifests;
            }
            finally 
            {
                if (latestTransactionInfo != null)
                    latestTransactionInfo.Unlock();
            }
        }


        public bool PackageNameInUse(string project, string id)
        {
            IEnumerable<string> rawList = this.GetManifestPointers(project);
            // remove "_manifest" and decloak, as we want to get package ids from file names
            rawList = rawList.Select(r => Obfuscator.Decloak(Path.GetFileName(r).Replace("_manifest", string.Empty)));

            return rawList.Contains(id);
        }


        public Package GetPackage(string project, string packageId)
        {
            ActiveTransaction latestTransactionInfo = this.GetActiveTransaction(project);

            try
            {

                if (latestTransactionInfo == null)
                    return null;

                string manifestPointerPath = Path.Combine(latestTransactionInfo.Info.FullName, $"{Obfuscator.Cloak(packageId)}_manifest");
                if (!File.Exists(manifestPointerPath))
                    throw new PackageNotFoundException(packageId);

                string manifestRealPath = Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment, File.ReadAllText(manifestPointerPath));

                string rawPackage;

                try
                {
                    rawPackage = File.ReadAllText(manifestRealPath);
                }
                catch (System.IO.FileNotFoundException)
                {
                    // there is no guarantee the package hasn't already been deleted, so must always gracefully handle
                    // missing file
                    throw new PackageNotFoundException(packageId);
                }

                try
                {
                    Package package = JsonConvert.DeserializeObject<Package>(rawPackage);
                    package.PathOnDisk = manifestRealPath;
                    return package;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error trying to read manifest @ {manifestRealPath}. Pointer path was {manifestPointerPath}.");
                    return null;
                }
            }
            finally 
            {
                if (latestTransactionInfo != null)
                    latestTransactionInfo.Unlock();
            }

        }


        public GetFileResponse GetFile(string project, string id)
        {
            FileIdentifier fileIdentifier = FileIdentifier.Decloak(id);
            IRehydrator rehydrator = _typeProvider.GetInstance<IRehydrator>();
            string binPath = rehydrator.RehydrateOrResolveFile(project, fileIdentifier.Package, fileIdentifier.Path);

            if (string.IsNullOrEmpty(binPath)) 
                throw new Tetrifact.Core.FileNotFoundException(fileIdentifier.Path);
            else
                return new GetFileResponse(new FileStream(binPath, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(fileIdentifier.Path));
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
            TimeSpan timeout = new TimeSpan(0, 0, Settings.ArchiveWaitTimeout);

            while (File.Exists(tempPath))
            {
                Thread.Sleep(Settings.ArchiveAvailablePollInterval);
                if (DateTime.Now - start > timeout)
                    throw new TimeoutException($"Timeout waiting for package ${packageId} archive to build");
            }

            return new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }


        // todo : make private
        public string GetArchivePath(string project, string packageId)
        {
            Package package = this.GetPackage(project, packageId);
            if (package == null)
                throw new PackageNotFoundException(packageId);

            return Path.Combine(Settings.ArchivePath, Obfuscator.Cloak(project), string.Format($"{package.UniqueId}.zip"));
        }


        public string GetTempArchivePath(string project, string packageId)
        {
            Package package = this.GetPackage(project, packageId);
            if (package == null)
                throw new PackageNotFoundException(packageId);

            return Path.Combine(Settings.ArchivePath, Obfuscator.Cloak(project), string.Format($"{package.UniqueId}.zip.tmp"));
        }


        public string GetHead(string project) 
        {
            ActiveTransaction activeTransaction = this.GetActiveTransaction(project);

            try
            {
                if (activeTransaction == null)
                    return null;

                string headPath = Path.Combine(activeTransaction.Info.FullName, "head");
                if (!File.Exists(headPath))
                    return null;

                return File.ReadAllText(headPath);
            }
            finally
            {
                if (activeTransaction != null)
                    activeTransaction.Unlock();
            }
        }


        private bool DoesPackageExist(string project, string packageId)
        {
            Package package = this.GetPackage(project, packageId);
            return package != null;
        }


        private void CreateArchive(string project, string packageId)
        {
            // store path with .tmp extension while building, this is used to detect if archiving has already started
            string archivePathTemp = this.GetTempArchivePath(project, packageId);


            if (!this.DoesPackageExist(project, packageId))
                throw new PackageNotFoundException(packageId);

            FileHelper.EnsureParentDirectoryExists(archivePathTemp);

            // if temp archive exists, it's already building
            if (File.Exists(archivePathTemp))
                return;

            // create zip file on disk asap to lock file name off
            try
            {
                using (FileStream zipStream = new FileStream(archivePathTemp, FileMode.Create))
                {
                    Package package = this.GetPackage(project, packageId);

                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in package.Files)
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
            }
            catch (IOException ex) 
            {
                // this should not occur because we're already testing for file locks, but if the tmp file exists and is locked, the archive must still be cooking
                if (ex.Message.Contains("The process cannot access the file"))
                    return;
            }


            // flip temp file to final path, it is ready for use only when this happens
            string archivePath = this.GetArchivePath(project, packageId);
            File.Move(archivePathTemp, archivePath);
        }

        #endregion
    }
}
