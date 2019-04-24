namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type that creates a package.
    /// </summary>
    public interface IPackageCreate
    {
        /// <summary>
        /// Creates a package
        /// </summary>
        /// <param name="newPackage"></param>
        /// <returns></returns>
        PackageCreateResult CreatePackage(PackageCreateArguments newPackage);
    }
}
