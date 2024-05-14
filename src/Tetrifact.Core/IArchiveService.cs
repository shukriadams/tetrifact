using System.IO;

namespace Tetrifact.Core
{
    public interface IArchiveService
    {
        /// <summary>
        /// Generates an archive for the given package id, sychronously. Returns when archive is generated. This method is exposed for testing purposes only, and must not tamper with archive queue. 
        /// Normally it will be called from CreateNextQueuedArchive().
        /// </summary>
        /// <param name="packageId"></param>
        void CreateArchive(string packageId);

        /// <summary>
        /// Generates the next archive in queue. Synchronous process, exits when archive generation is complete. Archive is removed from queue on exit.
        /// </summary>
        void CreateNextQueuedArchive();

        void CleanupNextQueuedArchive();

        /// <summary>
        /// Adds package to queue for creation. This process does not produce the archive itself, see CreateNextQueuedArchive for that. Queuing is used so archive generation can be handled 
        /// on its own thread.
        /// </summary>
        /// <param name="packageId"></param>
        void QueueArchiveCreation(string packageId);

        /// <summary>
        /// Cleans out trash in archives folder.
        /// </summary>
        void PurgeOldArchives();

        /// <summary>
        /// Gets a package as an archive. Assumes the archive already exists, throws archive not found exception if not. Does
        /// not verify package id validity.
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        Stream GetPackageAsArchive(string packageId);

        /// <summary>
        /// Returns a status code the given archive. Requesting status for a given archive will also start
        /// generating that archive.
        /// 0 : Archive creation was started.
        /// 1 : Archive is already started, is still in progress.
        /// 2 : Archive is available for download.
        /// 
        /// throw a PackageNotFoundException if the package does not exist or is marked for delete
        /// </summary>
        /// <returns></returns>
        ArchiveProgressInfo GetPackageArchiveStatus(string packageId);

        /// <summary>
        /// Gets the path for a package archive while archive is being generated. This file will be renamed to the
        /// final public package name when archive generation is done. Therefore, the existence of the temp file 
        /// is used as an indicator that archive creation is still in progress (or failed while in progress).
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        string GetPackageArchiveTempPath(string packageId);

        /// <summary>
        /// Gets the path for a package archive.
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        string GetPackageArchivePath(string packageId);

        string GetArchiveProgressKey(string packageId);
    }
}
