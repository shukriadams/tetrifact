using System.IO.Abstractions;
using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Base : TestBase
    {
        protected IPackageCreateWorkspace PackageCreateWorkspace;

        public Base()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();
            TestLogger<IPackageCreateWorkspace> workspaceLogger = new TestLogger<IPackageCreateWorkspace>();

            PackageCreateWorkspace = new PackageCreateWorkspace(settings, indexReader, fileSystem, workspaceLogger, HashServiceHelper.Instance());
            PackageCreateWorkspace.Initialize();
        }
    }
}
