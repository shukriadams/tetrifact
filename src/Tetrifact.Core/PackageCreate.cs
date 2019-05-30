using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetrifact.Core
{
    public class PackageCreate : IPackageCreate
    {
        #region FIELDS

        private IIndexReader _indexReader;
        private IWorkspace _workspace;
        private ITetriSettings _settings;
        private ILogger<IPackageCreate> _log;

        #endregion

        #region CTORS

        public PackageCreate(IIndexReader indexReader, ITetriSettings settings, ILogger<IPackageCreate> log, IWorkspace workspace)
        {
            _indexReader = indexReader;
            _settings = settings;
            _log = log;
            _workspace = workspace;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifest"></param>
        public PackageCreateResult CreatePackage(PackageCreateArguments newPackage)
        {
            List<string> transactionLog = new List<string>();
            StringBuilder hashes = new StringBuilder();

            try
            {
                // validate the contents of "newPackage" object
                if (!newPackage.Files.Any())
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Files collection is empty." };

                if (string.IsNullOrEmpty(newPackage.Id))
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.MissingValue, PublicError = "Id is required." };

                // ensure package does not already exist
                if (_indexReader.PackageNameInUse(newPackage.Id))
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.PackageExists };

                // if archive, ensure correct file count
                if (newPackage.IsArchive && newPackage.Files.Count() != 1)
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidFileCount };

                // if archive, ensure correct file format 
                if (newPackage.IsArchive && newPackage.Format != "zip")
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat };

                // write attachments to work folder 
                long size = newPackage.Files.Sum(f => f.Length);

                _workspace.Initialize();

                // if archive, unzip
                if (newPackage.IsArchive)
                    _workspace.AddArchiveContent(newPackage.Files.First().OpenReadStream());
                else
                    foreach (IFormFile formFile in newPackage.Files)
                        _workspace.AddIncomingFile(formFile.OpenReadStream(), formFile.FileName);

                // get all files which were uploaded, sort alphabetically for combined hashing
                string[] files = _workspace.GetIncomingFileNames().ToArray();
                Array.Sort(files, (x, y) => String.Compare(x, y));
                
                // prevent deletes of empty repository folders this package might need to write to
                LinkLock.Instance.Lock(newPackage.Id);

                foreach (string filePath in files)
                {
                    // get hash of incoming file
                    string fileHash = _workspace.GetIncomingFileHash(filePath);

                    hashes.Append(HashService.FromString(filePath));
                    hashes.Append(fileHash);

                    // todo : this would be a good place to confirm that existingPackageId is actually valid
                    _workspace.WriteFile(filePath, fileHash, newPackage.Id);
                }

                _workspace.Manifest.Description = newPackage.Description;

                // calculate package hash from child hashes
                _workspace.WriteManifest(newPackage.Id, HashService.FromString(hashes.ToString()));

                _workspace.Dispose();

                return new PackageCreateResult { Success = true, PackageHash = _workspace.Manifest.Hash };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, string.Empty);
                Console.WriteLine($"Unexpected error : {ex}");
                return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.UnexpectedError };
            }
            finally
            {
                if (!string.IsNullOrEmpty(newPackage.Id))
                    LinkLock.Instance.Unlock(newPackage.Id);
            }
        }

        #endregion
    }
}
