using System.IO;

namespace Tetrifact.Core
{
    public interface IArchiveService
    {
        /// <summary>
        /// Cleans out trash in archives folder.
        /// </summary>
        void PurgeOldArchives();

        /// <summary>
        /// Gets a package as an archive. Create the archive if the archive doesn't already exist.
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        Stream GetPackageAsArchive(string packageId);

        /// <summary>
        /// Returns a status code the given archive. Requesting status for a given archive will also start
        /// generating that archive.
        /// 0 : Archive creation started.
        /// 1 : Archive is being created.
        /// 2 : Archive is available for download.
        /// 
        /// throw a PackageNotFoundException if the package does not exist or is marked for delete
        /// </summary>
        /// <returns></returns>
        int GetPackageArchiveStatus(string packageId);

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
    }
}
