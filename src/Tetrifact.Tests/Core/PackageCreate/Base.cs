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
            Workspace = new PackageCreateWorkspace(Settings, base.FileSystem, new TestLogger<IPackageCreateWorkspace>(), HashServiceHelper.Instance());
            Logger = new TestLogger<IPackageCreateService>();

            PackageCreate = new Core.PackageCreateService(
                IndexReader, 
                ArchiveService,
                Settings,
                Logger,
                Workspace, 
                HashServiceHelper.Instance());
        }
    }
}
