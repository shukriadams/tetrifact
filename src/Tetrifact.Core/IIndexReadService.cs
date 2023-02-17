using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Presents an abstraction that can be used to write to and read from a collection of packages. In production this would 
    /// be a collection of files and folders on a file system. For testing purposes this would be an in-memory abstraction.
    /// </summary>
    public interface IIndexReadService
    {
        /// <summary>
        /// Returns true if package with given id currently exists
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        bool PackageExists(string packageId);

        /// <summary>
        /// Gets a list of all package ids in repository. This method is expensive at scale and should be used only when absolutely necessary.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllPackageIds();

        /// <summary>
        /// Gets a list of all packages which can be linked to. A package that cannot be linked to can still be downloaded from 
        /// through a file link from another package.
        /// 
        /// Packages are stored as immediate child folders of the system package folder. Package names are the short names of these folders,
        /// that is, if the server package folder contains package1 as /path/to/packages/package1, then "package1" would be the name of the
        /// available package.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetPackageIds(int pageIndex, int pageSize);

        /// <summary>
        /// create all required folders and stuctures.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Returns true if the package is being used. This doesn't mean the package is available, just that the package folder 
        /// exists.
        /// </summary>
        /// <returns></returns>
        bool PackageNameInUse(string id);

        Manifest GetManifest(string packageId);

        /// <summary>
        /// Returns the manifest of a given package if the package exists. Returns null if the package is invalid.
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        Manifest GetManifestHead(string packageId);

        /// <summary>
        /// Returns a manifest. Throws exception if manifest not found.
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        Manifest GetExpectedManifest(string packageId);

        /// <summary>
        /// Gets a file from a package, as a binary array. Returns null if the package or file is invalid.
        /// file fileIdentifier is file path + "::" + file hash, base64 encoded. Does not throw file not found exception
        /// because the severity of a null depends on the context of the call.
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        GetFileResponse GetFile(string fileIdentifier);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        void DeletePackage(string packageId);

        /// <summary>
        /// Verifies integrity of package by recalculating hash of all files in package and comparing it with manifest hash.
        /// Throws exception if hashes do not match.
        /// </summary>
        /// <param name="packageId"></param>
        (bool, string) VerifyPackage(string packageId);

        /// <summary>
        /// Gets object with reading of disk use.
        /// </summary>
        /// <returns></returns>
        DiskUseStats GetDiskUseSats();
    }
}
