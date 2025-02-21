using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
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

        private readonly IProcessManager _lock;
    
        private IList<string> _cleaned = new List<string>();

        private IList<string> _failed = new List<string>();

        private long _directoriesScanned = 0;

        private long _filesScanned = 0;

        private readonly IMemoryCache _cache;

        private IEnumerable<string> _existingPackageIds = new string[0];

        #endregion

        #region PROPERTIES

        public int LockPasses { private set; get; }

        #endregion

        #region CTORS

        public RepositoryCleanService(IIndexReadService indexReader, IMemoryCache cache, IProcessManager lockInstance, ISettings settings, IDirectory directoryFileSystem, IFile fileFileSystem, ILogger<IRepositoryCleanService> log)
        {
            _settings = settings;
            _directoryFileSystem = directoryFileSystem;
            _fileFilesystem = fileFileSystem;
            _log = log;
            _indexReader = indexReader;
            _lock = lockInstance;
            _cache = cache;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Starts clean process on repository. Process can be blocked while an existing upload is in progress.
        /// </summary>
        public CleanResult Clean()
        {
            string processUID = Guid.NewGuid().ToString();
            try 
            {
                // get a list of existing packages at time of calling. It is vital that new packages not be created
                // while clean running, they will be cleaned up as they are not on this list
                _existingPackageIds = _indexReader.GetAllPackageIds();
                if (EnsureNoLock(false))
                {
                    IEnumerable<ProcessItem> locks = _lock.GetByCategory(ProcessCategories.Package_Create);
                    return new CleanResult
                    {
                        Description = $"Package locks found, clean exited before start : ({string.Join(",", locks)})"
                    };

                }

                _lock.AddUnique(ProcessCategories.CleanRepository, processUID);
                _log.LogInformation($"CLEANUP started, {_existingPackageIds.Count()} package(s) present.");

                this.LockPasses = 0;
                this.Clean_Internal(_settings.RepositoryPath, false);

                return new CleanResult{ 
                    Cleaned = _cleaned, 
                    Failed = _failed, 
                    DirectoriesScanned = _directoriesScanned, 
                    FilesScanned = _filesScanned, 
                    PackagesInSystem = _existingPackageIds.Count(),
                    Description = "Clean completed"
                };
            }
            catch (Exception ex)
            { 
                if (ex.Message.StartsWith("System currently locked"))
                {
                    _log.LogInformation("Clean aborted, lock detected");
                    IEnumerable<ProcessItem> locks = _lock.GetByCategory(ProcessCategories.Package_Create);
                    return new CleanResult{
                        Cleaned = _cleaned, 
                        Failed = _failed, 
                        DirectoriesScanned = _directoriesScanned, 
                        FilesScanned = _filesScanned, 
                        PackagesInSystem = _existingPackageIds.Count(),
                        Description = $"Clean aborted, locked detected : ({string.Join(",", locks)}"
                    };
                }
                else
                    throw ex;
            }
            finally 
            {
                _lock.RemoveUnique(processUID);
            }
        }

        /// <summary>
        /// Call this before each destructive change for maximum resolution
        /// </summary>
        private bool EnsureNoLock(bool throwOnOLock = true)
        {
            if (_lock.AnyWithCategoryExists(ProcessCategories.Package_Create))
            { 
                if (throwOnOLock)
                    throw new Exception($"System currently locked, clear process aborting");
                else
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Recursing method behind Clean() logic. Wrap all FS interactions in try catches with IOException handlers as this method is very likely to throw exceptions when cascading deletes
        /// remove directories from above while recursing.
        /// </summary>
        /// <param name="currentDirectory"></param>
        private void Clean_Internal(string currentDirectory, bool currentDirectoryIsPackageSubscriberList)
        {
            EnsureNoLock();

            string[] files = null;
            string[] directories = null;
            _directoriesScanned ++;

            try
            {
                files = _directoryFileSystem.GetFiles(currentDirectory);
                directories = _directoryFileSystem.GetDirectories(currentDirectory);
            }
            catch (IOException ex)
            {
                _log.LogError($"Failed to read content of directory {currentDirectory} {ex}");
                _failed.Add(currentDirectory);
                // if we can't read the files or directories in the current path, skip it and try to clean up other paths
                return;
            }

            // file @ hash can be reserved for incoming partial uploads
            if (_cache.Get($"{currentDirectory}::LOOKUP_RESERVE::") != null)
            {
                _log.LogInformation($"Skipping clean for {currentDirectory}, lookup reserve detected.");
                return;
            }

            // Case 1
            // if directory is completely empty and it's not the root repo directory, we can safely delete it
            if (!files.Any() && !directories.Any() && currentDirectory != _settings.RepositoryPath)
            {
                try
                {
                    EnsureNoLock();
                    _directoryFileSystem.Delete(currentDirectory, true);
                    _cleaned.Add(currentDirectory);
                    _log.LogWarning($"CLEANUP : deleted directory {currentDirectory}, no children.");
                }
                catch (IOException ex)
                {
                    _log.LogError($"ERROR : Failed to delete directory {currentDirectory} {ex}");
                    _failed.Add(currentDirectory);
                }
            }

            // packages directory is the directory next to "bin" file that contains subscriber files for each package that refererences the bin
            if (currentDirectoryIsPackageSubscriberList)
            {
                // Case 2 : subscribed package files exist but the packages listed no longer exist
                // package directory contains files only, never directories, so we check file presence only
                if (files.Any())
                {
                    foreach (string file in files)
                    {
                        if (!_existingPackageIds.Contains(Path.GetFileName(file)))
                        {
                            try
                            {
                                EnsureNoLock();
                                _fileFilesystem.Delete(file);
                                _cleaned.Add(file);
                                _log.LogWarning($"CLEANUP : deleted file {file}, package not found.");
                            }
                            catch (IOException ex)
                            {
                                _log.LogError($"ERROR : Failed to delete file {file} {ex}");
                                _failed.Add(file);
                            }
                        }
                    }
                }
            }

            bool binFilePresent = files.Any(r => Path.GetFileName(r) == "bin");
            if (binFilePresent)
                _filesScanned ++;

            if (binFilePresent)
            {
                bool hasSubscribers = false;
                string subscriberDirectory = Path.Combine(currentDirectory, "packages");
                if (_directoryFileSystem.Exists(subscriberDirectory) && _directoryFileSystem.GetFiles(subscriberDirectory).Any())
                    hasSubscribers = true;

                if (!hasSubscribers)
                {
                    // Case 3 : bin has no subscribers
                    // if reach here there are no package link files in this directory, it is safe to attempt to delete it
                    // find the bin file in the parent directory associated with this package dir, and try to delete that
                    try
                    {
                        // delete this (package) directory 
                        EnsureNoLock();
                        _directoryFileSystem.Delete(currentDirectory, true);
                        _cleaned.Add(currentDirectory);
                        _log.LogWarning($"CLEANUP : deleted package directory {currentDirectory}, not associated with any packages.");
                    }
                    catch (IOException ex)
                    {
                        _log.LogError($"ERROR deleting bin file file {currentDirectory} {ex}");
                        _failed.Add(currentDirectory);
                    }

                    // done - package directory is deleted or failed to delete, no need to continue down this path
                    return;
                }
            }

            foreach (string childDirectory in directories)
            {
                // if the dir about to processed is called "packages" and there is a bin file present in current (parent) dir,
                // then child is the packages directory
                bool isPackageDir = Path.GetFileName(childDirectory) == "packages" && binFilePresent;
                Clean_Internal(childDirectory, isPackageDir);
            }
        }

        #endregion
    }
}
