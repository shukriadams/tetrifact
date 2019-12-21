using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Base type for tests which require package creation logic using the production API.
    /// </summary>
    public abstract class PackageCreatorBase : FileSystemBase
    {
        protected IPackageCreate PackageCreate;

        public PackageCreatorBase()
        {
            PackageCreate = new Core.PackageCreate(
                IndexReader,
                new TestLogger<IPackageCreate>(),
                this.Settings);
        }
    }
}
