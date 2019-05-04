using Tetrifact.Core;

namespace Tetrifact.Tests.PackageList
{
    public class Base : FileSystemBase
    {
        protected IPackageList PackageList { get; private set; }

        public Base()
        {
            this.PackageList = new Core.PackageList(null, Settings, new TestLogger<IPackageList>());
        }
    }
}
