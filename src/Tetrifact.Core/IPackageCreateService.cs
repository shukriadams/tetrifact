namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type that creates a package.
    /// </summary>
    public interface IPackageCreateService
    {
        /// <summary>
        /// Creates a package
        /// </summary>
        /// <param name="newPackage"></param>
        /// <returns></returns>
        PackageCreateResult Create(PackageCreateArguments newPackage);
    }
}
