using Tetrifact.Core;

namespace Tetrifact.Tests.PackageCreate
{
    public abstract class Base : TestBase
    {
        protected IPackageCreateService PackageCreate;

        public Base()
        {
            PackageCreate = MoqHelper.CreateInstanceWithDependencies<PackageCreateService>(new object[]{ new TestFileSystem() });
        }
    }
}
