using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Base : FileSystemBase
    {
        protected IPackageCreateWorkspace PackageCreateWorkspace;

        public Base()
        {
            PackageCreateWorkspace = new PackageCreateWorkspace(SettingsHelper.CurrentSettingsContext, base.IndexReader, base.FileSystem, base.WorkspaceLogger, HashServiceHelper.Instance());
            PackageCreateWorkspace.Initialize();
        }
    }
}
