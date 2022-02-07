using Tetrifact.Core;

namespace Tetrifact.Tests.PackageCreate
{
    public abstract class Base : FileSystemBase
    {
        protected IPackageCreate PackageCreate;
        protected IWorkspace Workspace;
        protected new TestLogger<IPackageCreate> Logger;

        public Base()
        {
            Workspace = new Core.Workspace(Settings, new TestLogger<IWorkspace>(), HashServiceHelper.Instance());
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
