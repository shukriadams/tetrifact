using Tetrifact.Core;

namespace Tetrifact.Tests.PackageCreate
{
    public abstract class Base : FileSystemBase
    {
        protected IPackageCreate PackageCreate;

        protected IPackageCreateWorkspace Workspace;

        protected TestLogger<IPackageCreate> Logger;

        public Base()
        {
            Workspace = new Core.PackageCreateWorkspace(Settings, new TestLogger<IPackageCreateWorkspace>(), HashServiceHelper.Instance());
            Logger = new TestLogger<IPackageCreate>();

            PackageCreate = new Core.PackageCreate(
                IndexReader, 
                ArchiveService,
                Settings,
                Logger,
                Workspace, 
                HashServiceHelper.Instance());
        }
    }
}
