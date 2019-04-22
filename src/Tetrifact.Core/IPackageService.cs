namespace Tetrifact.Core
{
    public interface IPackageService
    {
        /// <summary>
        /// Creates a package
        /// </summary>
        /// <param name="newPackage"></param>
        /// <returns></returns>
        PackageCreateResult CreatePackage(PackageCreateArguments newPackage);
    }
}
