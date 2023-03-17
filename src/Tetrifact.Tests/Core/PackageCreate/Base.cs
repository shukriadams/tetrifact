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
            Workspace = new PackageCreateWorkspace(Settings, IndexReader, base.FileSystem, new TestLogger<IPackageCreateWorkspace>(), HashServiceHelper.Instance());
            Logger = new TestLogger<IPackageCreateService>();

            PackageCreate = new PackageCreateService(
                IndexReader,
                LockProvider,
                ArchiveService,
                Settings,
                Logger,
                Workspace, 
                HashServiceHelper.Instance(),
                new TestFileSystem());
        }
    }
}
