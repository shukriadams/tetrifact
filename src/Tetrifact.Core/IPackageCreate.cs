using System.Threading.Tasks;

namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type that creates a package. Packages are created in three distinct ways:
    /// 1 - from a list of POSTed files
    /// 2 - from a POSTed archive containing a list of files
    /// 3 - from a list of files taken from an existing package
    /// 
    /// The first two methods require validation, the third bypasses checks as the files are from a package that has already been validated.
    /// 
    /// A package is created by staging its files in a temporary folder, the moving that folder to the shards folder.
    /// 
    /// </summary>
    public interface IPackageCreate
    {
        /// <summary>
        /// Creates a package
        /// </summary>
        /// <param name="project"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        Task<PackageCreateResult> CreatePackage(PackageCreateArguments package);
    }
}
