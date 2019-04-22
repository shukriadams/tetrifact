using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetrifact.Core
{
    public class PackageService : IPackageService
    {
        public IIndexReader IndexReader;
        private IWorkspaceProvider _packageWorkspaceProvider;
        private ITetriSettings _settings;
        private ILogger<IPackageService> _log;

        public PackageService(IIndexReader indexReader, ITetriSettings settings, ILogger<IPackageService> log, IWorkspaceProvider packageWorkspaceProvider)
        {
            this.IndexReader = indexReader;
            _settings = settings;
            _log = log;
            _packageWorkspaceProvider = packageWorkspaceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifest"></param>
        public PackageAddResult CreatePackage(PackageAddArgs newPackage)
        {
            List<string> transactionLog = new List<string>();
            IWorkspace workspace = null;
            StringBuilder hashes = new StringBuilder();

            try
            {
                // validate the contents of "newPackage" object
                if (newPackage.Files == null)
                    return new PackageAddResult { ErrorType = PackageAddErrorTypes.MissingValue, PublicError = "Expected 'Files' collection is missing." };

                if (string.IsNullOrEmpty(newPackage.Id))
                    return new PackageAddResult { ErrorType = PackageAddErrorTypes.MissingValue, PublicError = "Id is required" };

                // ensure package does not already exist
                if (this.IndexReader.PackageNameInUse(newPackage.Id))
                    return new PackageAddResult { ErrorType = PackageAddErrorTypes.PackageExists };

                // if archive, ensure correct file count
                if (newPackage.IsArchive && newPackage.Files.Count() == 0)
                    return new PackageAddResult { ErrorType = PackageAddErrorTypes.InvalidFileCount };

                // if archive, ensure correct file count
                if (newPackage.IsArchive && newPackage.Files.Count() != 1)
                    return new PackageAddResult { ErrorType = PackageAddErrorTypes.InvalidFileCount };

                // if archive, ensure correct file format 
                if (newPackage.IsArchive && !new string[] { "zip" }.Contains(newPackage.Format))
                    return new PackageAddResult { ErrorType = PackageAddErrorTypes.InvalidArchiveFormat };

                workspace = _packageWorkspaceProvider.Get();

                // write attachments to work folder 
                long size = newPackage.Files.Sum(f => f.Length);

                // if archive, unzip
                if (newPackage.IsArchive)
                    workspace.AddArchiveContent(newPackage.Files.First().OpenReadStream());
                else
                    foreach (IFormFile formFile in newPackage.Files)
                        workspace.AddIncomingFile(formFile.OpenReadStream(), formFile.FileName);

                // get all files which were uploaded, sort alphabetically for combined hashing
                string[] files = workspace.GetIncomingFileNames().ToArray();
                Array.Sort(files, (x, y) => String.Compare(x, y));
                
                // prevent deletes of empty repository folders this package might need to write to
                LinkLock.Instance.Lock(newPackage.Id);

                foreach (string filePath in files)
                {
                    // get hash of incoming file
                    string fileHash = workspace.GetIncomingFileHash(filePath);

                    hashes.Append(HashService.FromString(filePath));
                    hashes.Append(fileHash);

                    // todo : this would be a good place to confirm that existingPackageId is actually valid
                    workspace.WriteFile(filePath, fileHash, newPackage.Id);
                }

                workspace.Manifest.Description = newPackage.Description;

                // calculate package hash from child hashes
                workspace.WriteManifest(newPackage.Id, HashService.FromString(hashes.ToString()));

                return new PackageAddResult { Success = true, PackageHash = workspace.Manifest.Hash };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, string.Empty);
                Console.WriteLine(string.Format("Unexpected error : {0}", ex));
                return new PackageAddResult { ErrorType = PackageAddErrorTypes.UnexpectedError };
            }
            finally
            {
                LinkLock.Instance.Unlock(newPackage.Id);
            }
        }
    }
}
