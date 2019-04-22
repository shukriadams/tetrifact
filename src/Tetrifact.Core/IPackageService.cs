using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public interface IPackageService
    {
        /// <summary>
        /// Creates a package
        /// </summary>
        /// <param name="newPackage"></param>
        /// <returns></returns>
        Task<PackageAddResult> CreatePackageAsync(PackageAddArgs newPackage);
    }
}
