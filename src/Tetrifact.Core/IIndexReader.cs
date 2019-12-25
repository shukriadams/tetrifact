using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    /// <summary>
    /// Presents an abstraction that can be used to write to and read from a collection of packages. In production this would 
    /// be a collection of files and folders on a file system. For testing purposes this would be an in-memory abstraction.
    /// </summary>
    public interface IIndexReader
    {

        /// <summary>
        /// Gets the directory info for the folder containing the latest and therefore current transaction. Returns null if no
        /// transactions are committed yet.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        DirectoryInfo GetActiveTransactionInfo(string project);

        /// <summary>
        /// Gets x nr of latest transactions including active.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        IEnumerable<DirectoryInfo> GetLatestTransactionsInfo(string project, int count);

        /// <summary>
        /// Gets a list of all manifest pointers in a project. These are the pointer files - they must be loaded to get all manifests
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        IEnumerable<string> GetManifestPaths(string project);

        /// <summary>
        /// Gets a list of all package ids in repository. This method is expensive at scale and should be used only when absolutely necessary.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllPackageIds(string project);

        /// <summary>
        /// Gets a list of all packages which can be linked to. A package that cannot be linked to can still be downloaded from 
        /// through a file link from another package.
        /// 
        /// Packages are stored as immediate child folders of the system package folder. Package names are the short names of these folders,
        /// that is, if the server package folder contains package1 as /path/to/packages/package1, then "package1" would be the name of the
        /// available package.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetPackageIds(string project, int pageIndex, int pageSize);

        /// <summary>
        /// Returns true if the package is being used. This doesn't mean the package is available, just that the package folder 
        /// exists.
        /// </summary>
        /// <returns></returns>
        bool PackageNameInUse(string project, string package);

        /// <summary>
        /// Returns the manifest of a given package if the package exists. Returns null if the package is invalid.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        Manifest GetManifest(string project, string package);

        /// <summary>
        /// Gets a file from a package, as a binary array. Returns null if the package or file is invalid.
        /// file fileIdentifier is file path + "::" + file hash, base64 encoded.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        GetFileResponse GetFile(string project, string fileIdentifier);

        /// <summary>
        /// Gets a package as an archive. Create the archive if the archive doesn't already exist.
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        Stream GetPackageAsArchive(string project, string package);

        /// <summary>
        /// Cleans out trash in archives folder.
        /// </summary>
        void PurgeOldArchives();

        /// <summary>
        /// Gets the path for a package archive while archive is being generated. This file will be renamed to the
        /// final public package name when archive generation is done. Therefore, the existence of the temp file 
        /// is used as an indicator that archive creation is still in progress (or failed while in progress).
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        string GetPackageArchiveTempPath(string project, string package);

        /// <summary>
        /// Gets the path for a package archive.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        string GetPackageArchivePath(string project, string package);

        /// <summary>
        /// Gets the id of the package at the head of the given project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        string GetHead(string project);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="package"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        string RehydrateOrResolveFile(string project, string package, string filePath);

        /// <summary>
        /// Gets a list of all projects currently created on server.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetProjects();

        /// <summary>
        /// Gets the absolute path of an item if it exists.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="package"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetItemPathOnDisk(string project, string package, string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        bool ProjectExists(string project);
    }
}
