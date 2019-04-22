namespace Tetrifact.Core
{
    public interface IPackageService
    {
        /// <summary>
        /// Creates a package
        /// </summary>
        /// <param name="newPackage"></param>
        /// <returns></returns>
        PackageAddResult CreatePackage(PackageAddArgs newPackage);
    }
}
