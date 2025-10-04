using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Base : TestBase
    {
        protected IPackageCreateWorkspace PackageCreateWorkspace;

        public Base()
        {
            PackageCreateWorkspace = TestContext.Get<IPackageCreateWorkspace>();
            PackageCreateWorkspace.Initialize();
        }
    }
}
