using System.IO.Abstractions;
using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Base : FileSystemBase
    {
        protected IPackageCreateWorkspace PackageCreateWorkspace;

        public Base()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            PackageCreateWorkspace = new PackageCreateWorkspace(settings, indexReader, fileSystem, base.WorkspaceLogger, HashServiceHelper.Instance());
            PackageCreateWorkspace.Initialize();
        }
    }
}
