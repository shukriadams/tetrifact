using System.Threading.Tasks;

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
        /// <param name="project"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        Task<PackageCreateResult> CreatePackage(PackageCreateArguments package);
    }
}
