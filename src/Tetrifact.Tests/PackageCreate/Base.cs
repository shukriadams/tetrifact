using Tetrifact.Core;

namespace Tetrifact.Tests.PackageCreate
{
    public abstract class Base : FileSystemBase
    {
        protected IPackageCreate PackageCreate;

        public Base()
        {
            PackageCreate = new Core.PackageCreate(IndexReader, Settings, new TestLogger<IPackageCreate>(), new Core.Workspace(Settings));
        }
    }
}
