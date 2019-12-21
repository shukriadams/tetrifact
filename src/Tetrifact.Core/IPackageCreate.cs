using System;
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
        Task<PackageCreateResult> CreateWithValidation(PackageCreateArguments package);

        /// <summary>
        /// Creates a package from an existing package. This must always be done against a reference package, and is intended for package deleting.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="package"></param>
        /// <param name="referencePackage"></param>
        /// <param name="transaction"></param>
        void CreateFromExisting(string project, string package, string referencePackage, Transaction transaction);
    }
}
