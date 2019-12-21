using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
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

                // if branchFrom package is specified, ensure that package exists (read its manifest as proof)
                if (!string.IsNullOrEmpty(newPackage.BranchFrom) && _indexReader.GetManifest(newPackage.Project, newPackage.BranchFrom) == null) 
                    return new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidDiffAgainstPackage };

                _workspace.Initialize(newPackage.Project);

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

                    _workspace.AddArchive(archiveFile, archiveType);
                }
                else
                    foreach (IFormFile formFile in newPackage.Files)
                        _workspace.AddFile(formFile.OpenReadStream(), formFile.FileName);
                
                _workspace.StageAllFiles(newPackage.Id, newPackage.BranchFrom);

                _workspace.Manifest.Description = newPackage.Description;

                // we calculate package hash from a sum of all child hashes
                _workspace.Commit(newPackage.Project, newPackage.Id, newPackage.BranchFrom, null);

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
