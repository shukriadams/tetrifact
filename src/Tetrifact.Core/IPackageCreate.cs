namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type that creates a package. Packages are created in four distinct ways:
    /// 1 - from a list of POSTed files
    /// 2 - from a POSTed archive containing a list of files
    /// 3 - from a list of files taken from an existing package (when a parent package is deleted, its child inherits all its file content)
    /// 4 - when optimizing a package by diffing it against parent (the opposite of 3 above, the child loses all file content which exists in parent)
    /// 
    /// The first two methods require validation, the third bypasses checks as the files are from a package that has already been validated.
    /// 
    /// A package is created by staging its files in a temporary folder, the moving that folder to the shards folder.
    /// 
    /// </summary>
    public interface IPackageCreate
    {
        /// <summary>
        /// Creates a new package, typically from uploaded files.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        PackageCreateResult Create(PackageCreateArguments package);

        /// <summary>
        /// Creates a package from an existing package. This must always be done against a reference package, and is used when a package is deleted and its child is recreated from the deleted
        /// parents' content.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="packageName"></param>
        /// <param name="transaction">Transaction to create package in. </param>
        /// <param name="parentPackageName">Name of parent package to compare to. OPTIONAL.</param>
        void CreateFromExisting(string project, string packageName, Transaction transaction, string parentPackageName);

        /// <summary>
        /// Creates a diffed version of the package, from itself.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="package"></param>
        void CreateDiffed(string project, string package);
    }
}
