using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Tetrifact.Core
{
    public class RepositoryCleanService : IRepositoryCleanService
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<IRepositoryCleanService> _log;

        private readonly IIndexReadService _indexReader;

        private readonly IDirectory _directoryFileSystem;
        
        private readonly IFile _fileFilesystem;

        private readonly ILock _lock;

        public int LockPasses {private set ; get;}

        #endregion

        #region CTORS

        public RepositoryCleanService(IIndexReadService indexReader, ILockProvider lockerProvider, ISettings settings, IDirectory directoryFileSystem, IFile fileFileSystem, ILogger<IRepositoryCleanService> log)
        {
            _settings = settings;
            _directoryFileSystem = directoryFileSystem;
            _fileFilesystem = fileFileSystem;
            _log = log;
            _indexReader = indexReader;
            _lock = lockerProvider.Instance;
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
                _log.LogInformation($"CLEANUP started, existing packages : {string.Join(",", existingPackageIds)}.");
                this.LockPasses = 0;
                this.Clean_Internal(_settings.RepositoryPath, existingPackageIds, false);
            }
            catch(Exception ex)
            { 
                if (ex.Message.StartsWith("System currently locked"))
                    _log.LogInformation("Clean aborted, lock detected");
                else
                    throw ex;
            }
            finally
            {
                _log.LogInformation($"CLEANUP complete");
            }
        }

        /// <summary>
        /// Call this before each destructive change for maximum resolution
        /// </summary>
        private void EnsureNoLock()
        {
            if (_lock.IsAnyLocked())
                throw new Exception($"System currently locked, clear process aborting");
        }

        /// <summary>
        /// Recursing method behind Clean() logic. Wrap all FS interactions in try catches with IOException handlers as this method is very likely to throw exceptions when cascading deletes
        /// remove directories from above while recursing.
        /// </summary>
        /// <param name="currentDirectory"></param>
        private void Clean_Internal(string currentDirectory, IEnumerable<string> existingPackageIds, bool currentDirectoryIsPackages)
        {
            EnsureNoLock();

            string[] files = null;
            string[] directories = null;

            try
            {
                files = _directoryFileSystem.GetFiles(currentDirectory);
                directories = _directoryFileSystem.GetDirectories(currentDirectory);
            }
            catch (IOException ex)
            {
                _log.LogError($"Failed to read content of directory {currentDirectory} ", ex);
                // if we can't read the files or directories in the current path, skip it and try to clean up other paths
                return;
            }

            // if directory is completely empty and it's not the root repo directory, we can safely delete it
            if (!files.Any() && !directories.Any() && currentDirectory != _settings.RepositoryPath)
            {
                try
                {
                    EnsureNoLock();
                    _directoryFileSystem.Delete(currentDirectory);
                    _log.LogWarning($"CLEANUP : deleted directory {currentDirectory}, no children.");
                }
                catch (IOException ex)
                {
                    _log.LogError($"ERROR : Failed to delete directory {currentDirectory} ", ex);
                }
            }
                
            // packages directory is the directory next to "bin" file that contains subscriber files for each package that refererences the bin
            if (currentDirectoryIsPackages)
            {
                // package directory contains files only, never directories, so we check file presence only
                if (files.Any())
                {
                    foreach (string file in files)
                    {
                        if (!existingPackageIds.Contains(Path.GetFileName(file)))
                        {
                            try
                            {
                                EnsureNoLock();
                                _fileFilesystem.Delete(file);
                                _log.LogWarning($"CLEANUP : deleted file {file}, package not found.");
                            }
                            catch (IOException ex)
                            {
                                _log.LogError($"ERROR : Failed to delete file {file} ", ex);
                            }
                        }
                    }
                }
                else
                {
                    // if reach here there are no package link files in this directory, it is safe to attempt to delete it

                    // find the bin file in the parent directory associated with this package dir, and try to delete that
                    string binFilePath = Path.Join(Directory.GetParent(currentDirectory).FullName, "bin");

                    try 
                    {
                        EnsureNoLock();
                        _fileFilesystem.Delete(binFilePath);
                        _log.LogWarning($"CLEANUP : deleted bin {binFilePath}, not associated with any packages.");
                    }
                    catch (IOException ex)
                    {
                        _log.LogError($"ERROR deleting bin file {binFilePath} ", ex);
                    }

                    try
                    {
                        // delete this (package) directory, yes 
                        EnsureNoLock();
                        _directoryFileSystem.Delete(currentDirectory);
                        _log.LogWarning($"CLEANUP : deleted package directory {currentDirectory}, not associated with any packages.");
                    }
                    catch (IOException ex)
                    {
                        _log.LogError($"ERROR deleting bin file file {currentDirectory} ", ex);
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
                    _fileFilesystem.Delete(filePath);
                    _log.LogWarning($"CLEANUP : deleted orphaned bin {filePath}, not associated with any packages.");
                }
                catch (IOException ex)
                {
                    _log.LogError($"ERROR Failed to delete file {filePath} ", ex);
                }

                return;
            }

            foreach (string childDirectory in directories)
            {
                // if the dir about to processed is called "packages" and there is a bin file present in current (parent) dir,
                // then child is the packages directory
                bool isPackageDir = Path.GetFileName(childDirectory) == "packages" && binFilePresent;
                Clean_Internal(childDirectory, existingPackageIds, isPackageDir);
            }
        }

        #endregion
    }
}
