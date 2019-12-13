using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public class PackageCreate : IPackageCreate
    {
        #region FIELDS

        private readonly IIndexReader _indexReader;
        private readonly IWorkspace _workspace;
        private readonly ILogger<IPackageCreate> _log;

        #endregion

        #region CTORS

        public PackageCreate(IIndexReader indexReader, ILogger<IPackageCreate> log, IWorkspace workspace)
        {
            _indexReader = indexReader;
            _log = log;
            _workspace = workspace;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifest"></param>
        public async Task<PackageCreateResult> CreatePackage(PackageCreateArguments newPackage)
        {
            List<string> transactionLog = new List<string>();
            string[] allowedFormats = new string[] { "zip", "tar.gz" };
            LockRequest lockRequest = new LockRequest();

            try
            {
                // wait until current write process is free
                await lockRequest.Get();

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

                // determine archive format from file extension
                if (newPackage.IsArchive) { 
                    newPackage.Format = Path.GetExtension(newPackage.Files[0].FileName);
                    if (newPackage.Format.StartsWith("."))
                        newPackage.Format = newPackage.Format.Substring(1);
                }

                // if archive, ensure correct file format 
                if (newPackage.IsArchive && !allowedFormats.Contains(newPackage.Format))
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat };

                // if branchFrom package is specified, ensure that package exists (read its manifest as proof)
                if (!string.IsNullOrEmpty(newPackage.BranchFrom) && _indexReader.GetManifest(newPackage.Project, newPackage.BranchFrom) == null) 
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidDiffAgainstPackage };

                // write attachments to work folder 
                long size = newPackage.Files.Sum(f => f.Length);

                _workspace.Initialize(newPackage.Project);

                // if archive, unzip
                if (newPackage.IsArchive) {
                    IFormFile incomingArchive = newPackage.Files.First();
                    Stream archiveFile = incomingArchive.OpenReadStream();
                    string extension = Path.GetExtension(incomingArchive.FileName).Replace(".", string.Empty).ToLower();

                    // if archive, ensure correct file format 
                    extension = string.IsNullOrEmpty(extension) ? newPackage.Format : extension;
                    if (string.IsNullOrEmpty(extension))
                        return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat };

                    if (extension == "zip")
                        _workspace.AddZipContent(archiveFile);
                    else if (extension == "gz")
                        _workspace.AddTarContent(archiveFile);
                    else
                        return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat };
                }
                else
                    foreach (IFormFile formFile in newPackage.Files)
                        _workspace.AddIncomingFile(formFile.OpenReadStream(), formFile.FileName);
                
                _workspace.StageAllFiles(newPackage.Id, newPackage.BranchFrom);

                _workspace.Manifest.Description = newPackage.Description;

                // we calculate package hash from a sum of all child hashes
                _workspace.Finalize(newPackage.Project, newPackage.Id, newPackage.BranchFrom);

                _workspace.Dispose();

                return new PackageCreateResult { Success = true, PackageHash = _workspace.Manifest.Hash };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                Console.WriteLine($"Unexpected error : {ex}");
                return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.UnexpectedError };
            }
            finally
            {
                LinkLock.Instance.Release();
            }
        }

        #endregion
    }
}
