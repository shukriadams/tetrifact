using Tetrifact.Core;

namespace Tetrifact.Tests.PackageCreate
{
    public abstract class Base : FileSystemBase
    {
        protected IPackageCreateService PackageCreate;

        protected IPackageCreateWorkspace Workspace;

        protected TestLogger<IPackageCreateService> Logger;

        public Base()
        {
            Workspace = new PackageCreateWorkspace(SettingsHelper.CurrentSettingsContext, IndexReader, base.FileSystem, new TestLogger<IPackageCreateWorkspace>(), HashServiceHelper.Instance());
            Logger = new TestLogger<IPackageCreateService>();
            PackageCreate = MoqHelper.CreateInstanceWithDependencies<PackageCreateService>(new object[]{ SettingsHelper.CurrentSettingsContext, new TestFileSystem() });
        }
    }
}
