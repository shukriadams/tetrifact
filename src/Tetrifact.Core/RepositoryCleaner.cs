using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Tetrifact.Core
{
    public class RepositoryCleaner : IRepositoryCleaner
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<IRepositoryCleaner> _logger;

        private readonly IIndexReader _indexReader;

        private readonly IFileSystem _fileSystem;

        public int LockPasses {private set ; get;}

        #endregion

        #region CTORS

        public RepositoryCleaner(IIndexReader indexReader, ISettings settings, IFileSystem fileSystem, ILogger<IRepositoryCleaner> logger)
        {
            _settings = settings;
            _fileSystem = fileSystem;
            _logger = logger;
            _indexReader = indexReader;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Starts clean process on repository. Process can be blocked while an existing upload is in progress.
        /// </summary>
        public void Clean()
        {
            try 
            {
                // get a list of existing packages at time of calling. It is vital that new packages not be created
                // while clean running, they will be cleaned up as they are not on this list
                IEnumerable<string> existingPackageIds = _indexReader.GetAllPackageIds();
                _logger.LogInformation($"CLEANUP started, existing packages : {string.Join(",", existingPackageIds)}.");
                this.LockPasses = 0;
                this.Clean_Internal(_settings.RepositoryPath, existingPackageIds, false);
            }
            catch(Exception ex)
            { 
                if (ex.Message.StartsWith("System currently locked"))
                    _logger.LogInformation("Clean aborted, lock detected");
                else
                    throw ex;
            }
            finally
            {
                _logger.LogInformation($"CLEANUP complete");
            }
        }

        /// <summary>
        /// Call this before each destructive change for maximum resolution
        /// </summary>
        private void EnsureNoLock()
        {
            if (LinkLock.Instance.IsAnyLocked())
                throw new Exception($"System currently locked, clear process aborting");
        }

        /// <summary>
        /// Recursing method behind Clean() logic. Wrap all FS interactions in try catches with IOException handlers as this method is very likely to throw exceptions when cascading deletes
        /// remove directories from above while recursing.
        /// </summary>
        /// <param name="currentDirectory"></param>
        private void Clean_Internal(string currentDirectory, IEnumerable<string> existingPackageIds, bool isCurrentDirectoryPackages)
        {
            EnsureNoLock();

            string[] files = null;
            string[] directories = null;

            try
            {
                files = _fileSystem.Directory.GetFiles(currentDirectory);
                directories = _fileSystem.Directory.GetDirectories(currentDirectory);
            }
            catch (IOException ex)
            {
                _logger.LogError($"Failed to read content of directory {currentDirectory} ", ex);
                // if we can't read the files or directories in the current path, skip it and try to clean up other paths
                return;
            }

            // if directory is completely empty, we can safely delete it
            if (!files.Any() && !directories.Any() && currentDirectory != _settings.RepositoryPath)
            {
                try
                {
                    EnsureNoLock();
                    _fileSystem.Directory.Delete(currentDirectory);
                    _logger.LogWarning($"CLEANUP : deleted directory {currentDirectory}, no children.");
                }
                catch (IOException ex)
                {
                    _logger.LogError($"Failed to delete directory {currentDirectory} ", ex);
                }
            }
                
            // packages directory is the directory next to "bin" file that contains subscriber files for each package that refererences the bin
            if (isCurrentDirectoryPackages)
            {
                // package directory contains files only, never directories, so we check files only
                if (files.Any())
                {
                    foreach (string file in files)
                    {
                        if (!existingPackageIds.Contains(Path.GetFileName(file)))
                        {
                            try
                            {
                                EnsureNoLock();
                                _fileSystem.File.Delete(file);
                                _logger.LogWarning($"CLEANUP : deleted file {file}, package not found.");
                            }
                            catch (IOException ex)
                            {
                                _logger.LogError($"Failed to delete file {file} ", ex);
                            }
                        }
                    }
                }
                else
                {
                    // if reach here there are no packages in this packge directory, it is safe to attempt to delete it

                    // find the bin file in the parent directory associated with this package dir, and try to delete that
                    string binFilePath = Path.Join(Directory.GetParent(currentDirectory).FullName, "bin");
                    try 
                    {
                        if (_fileSystem.File.Exists(binFilePath))
                        {
                            EnsureNoLock();
                            _fileSystem.File.Delete(binFilePath);
                            _logger.LogWarning($"CLEANUP : deleted bin {binFilePath}, not associated with any packages.");
                        }
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError($"Error deleting bin file {binFilePath} ", ex);
                    }

                    try
                    {
                        // delete this the package directory
                        if (_fileSystem.Directory.Exists(currentDirectory))
                        {
                            EnsureNoLock();
                            _fileSystem.Directory.Delete(currentDirectory);
                            _logger.LogWarning($"CLEANUP : deleted package directory {currentDirectory}, not associated with any packages.");
                        }
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError($"Error deleting bin file file {currentDirectory} ", ex);
                    }

                    // done - package directory is deleted, no need to continue
                    return;
                }
            }


            bool binFilePresent = files.Any(r => Path.GetFileName(r) == "bin");
            if (binFilePresent && !directories.Any())
            {
                // bin file is orphaned (no package, no package folders)
                string filePath = Path.Join(currentDirectory, "bin");

                try
                {
                    EnsureNoLock();
                    _fileSystem.File.Delete(filePath);
                    _logger.LogWarning($"CLEANUP : deleted orphaned bin {filePath}, not associated with any packages.");
                }
                catch (IOException ex)
                {
                    _logger.LogError($"Failed to delete file {filePath} ", ex);
                }

                return;
            }

            foreach (string childDirectory in directories)
            {
                bool isPackageFolder = Path.GetFileName(childDirectory) == "packages" && binFilePresent;
                Clean_Internal(childDirectory, existingPackageIds, isPackageFolder);
            }
        }

        #endregion
    }
}
