using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tetrifact.Core
{
    public class PackageCreate : IPackageCreate
    {
        #region FIELDS

        private readonly IIndexReader _indexReader;

        private readonly IWorkspace _workspace;

        private readonly ILogger<IPackageCreate> _log;

        private readonly ITetriSettings _settings;

        private readonly IHashService _hashService;

        #endregion

        #region CTORS

        public PackageCreate(IIndexReader indexReader, ITetriSettings settings, ILogger<IPackageCreate> log, IWorkspace workspace, IHashService hashService)
        {
            _indexReader = indexReader;
            _log = log;
            _workspace = workspace;
            _settings = settings;
            _hashService = hashService;
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

                // write attachments to work folder 
                long size = newPackage.Files.Sum(f => f.Content.Length);

                _workspace.Initialize();

                // if archive, unzip
                if (newPackage.IsArchive)
                    _workspace.AddArchiveContent(newPackage.Files.First().Content);
                else
                    foreach (PackageCreateItem formFile in newPackage.Files)
                        _workspace.AddIncomingFile(formFile.Content, formFile.FileName);

                // get all files which were uploaded, sort alphabetically for combined hashing
                string[] files = _workspace.GetIncomingFileNames().ToArray();
                files = _hashService.SortFileArrayForHashing(files);
                
                // prevent deletes of empty repository folders this package might need to write to
                LinkLock.Instance.Lock(newPackage.Id);

                foreach (string filePath in files)
                {
                    // get hash of incoming file
                    string fileHash = _workspace.GetIncomingFileHash(filePath);

                    hashes.Append(_hashService.FromString(filePath));
                    hashes.Append(fileHash);

                    // todo : this would be a good place to confirm that existingPackageId is actually valid
                    _workspace.WriteFile(filePath, fileHash, newPackage.Id);
                }

                _workspace.Manifest.Description = newPackage.Description;

                // calculate package hash from child hashes
                _workspace.WriteManifest(newPackage.Id, _hashService.FromString(hashes.ToString()));

                _workspace.Dispose();

                if (_settings.AutoCreateArchiveOnPackageCreate)
                    using(Stream stream = _indexReader.GetPackageAsArchive(newPackage.Id)){ }

                return new PackageCreateResult { Success = true, PackageHash = _workspace.Manifest.Hash };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, string.Empty);
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
