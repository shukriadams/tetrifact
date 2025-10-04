using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Base : FileSystemBase
    {
        protected IPackageCreateWorkspace PackageCreateWorkspace;

        public Base()
        {
            ISettings settings = TestContext.Get<ISettings>();

            PackageCreateWorkspace = new PackageCreateWorkspace(settings, base.IndexReader, base.FileSystem, base.WorkspaceLogger, HashServiceHelper.Instance());
            PackageCreateWorkspace.Initialize();
        }
    }
}
