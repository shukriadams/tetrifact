using System.IO.Abstractions;
using Tetrifact.Core;

namespace Tetrifact.Tests.PackageCreate
{
    public abstract class Base : TestBase
    {
        protected IPackageCreateService PackageCreate;

        protected IPackageCreateWorkspace Workspace;

        protected TestLogger<IPackageCreateService> Logger;

        public Base()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            Workspace = new PackageCreateWorkspace(settings, indexReader, fileSystem, new TestLogger<IPackageCreateWorkspace>(), HashServiceHelper.Instance());
            Logger = new TestLogger<IPackageCreateService>();
            PackageCreate = MoqHelper.CreateInstanceWithDependencies<PackageCreateService>(new object[]{ new TestFileSystem() });
        }
    }
}
