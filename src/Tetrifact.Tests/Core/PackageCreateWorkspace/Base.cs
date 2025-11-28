using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Base
    {
        private readonly TestContext _testContext = new TestContext();

        protected IPackageCreateWorkspace PackageCreateWorkspace;

        public Base()
        {
            PackageCreateWorkspace = _testContext.Get<IPackageCreateWorkspace>();
            PackageCreateWorkspace.Initialize();
        }
    }
}
