using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tetrifact.Core
{
    public class RepositoryCleaner : IRepositoryCleaner
    {
        #region FIELDS

        private readonly ITetriSettings _settings;

        private readonly ILogger<IRepositoryCleaner> _logger;

        private readonly IIndexReader _indexReader;

        public int LockPasses {private set ; get;}

        #endregion

        #region CTORS

        public RepositoryCleaner(IIndexReader indexReader, ITetriSettings settings, ILogger<IRepositoryCleaner> logger)
        {
            _settings = settings;
            _logger = logger;
            _indexReader = indexReader;
        }

        #endregion

        #region METHODS

        public void Clean()
        {
            // get a list of existing packages at time of calling. It is vital that new packages not be created
            // while clean running, they will be cleaned up as they are not on this list
            IEnumerable<string> existingPackageIds = _indexReader.GetAllPackageIds();
            this.LockPasses = 0;
            this.Clean_Internal(_settings.RepositoryPath, existingPackageIds, false);
        }

        /// <summary>
        /// Recursing method behind Clean() logic. Wrap all FS interactions in try catches with IOException handlers as this method is very likely to throw exceptions when cascading deletes
        /// remove directories from above while recursing.
        /// </summary>
        /// <param name="currentDirectory"></param>
        private void Clean_Internal(string currentDirectory, IEnumerable<string> existingPackageIds, bool isCurrentDirectoryPackages)
        {
            // todo : add max sleep time to prevent permalock
            // wait if linklock is active, something more important is busy. 
            while (LinkLock.Instance.IsLocked())
            {
                this.LockPasses ++;
                Thread.Sleep(_settings.LinkLockWaitTime);
            }

            string[] files = null;
            string[] directories = null;

            try
            {
                files = Directory.GetFiles(currentDirectory);
                directories = Directory.GetDirectories(currentDirectory);
            }
            catch (IOException ex)
            {
                if (Directory.Exists(currentDirectory)) { 
                    _logger.LogError($"Failed to read content of directory ${currentDirectory} ", ex);
                    return;
                }
            }

            // if no children at all, delete current node
            if (!files.Any() && !directories.Any() && currentDirectory != _settings.RepositoryPath)
            {
                try
                {
                    Directory.Delete(currentDirectory);
                }
                catch (IOException ex)
                {
                    if (Directory.Exists(currentDirectory))
                        _logger.LogError($"Failed to delete directory ${currentDirectory} ", ex);
                }
            }
                

            if (isCurrentDirectoryPackages)
            {
                if (files.Any())
                {
                    foreach (string file in files)
                    {
                        if (!existingPackageIds.Contains(Path.GetFileName(file)))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (IOException ex)
                            {
                                _logger.LogError($"Failed to delete file ${file} ", ex);
                            }
                        }
                    }
                }
                else
                {
                    // if this is a package folder with no packages, it is safe to delete it and it's parent bin file
                    string binFilePath = Path.Join(Directory.GetParent(currentDirectory).FullName, "bin");
                    try 
                    {
                        if (File.Exists(binFilePath))
                            File.Delete(binFilePath);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError($"Error deleting bin file file ${binFilePath} ", ex);
                    }

                    try
                    {
                        if (Directory.Exists(currentDirectory))
                            Directory.Delete(currentDirectory);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError($"Error deleting bin file file ${currentDirectory} ", ex);
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
                    File.Delete(filePath);
                }
                catch (IOException ex)
                {
                    _logger.LogError($"Failed to delete file ${filePath} ", ex);
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
