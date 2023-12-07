using System.Collections.Generic;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class ListAvailablePackages : FileSystemBase
    {
        [Fact]
        public void ListAll()
        {
            TestPackage package1 = PackageHelper.CreateNewPackage(this.Settings);
            TestPackage package2 = PackageHelper.CreateNewPackage(this.Settings);
            TestPackage package3 = PackageHelper.CreateNewPackage(this.Settings);

            IPackageListService listService = NinjectHelper.Get<IPackageListService>(this.Settings);
            IEnumerable<Package> packages = listService.Get(0, 10);
            
            Assert.Equal(3, packages.Count());
            Assert.True(packages.Any(p => p.Id == package1.Id));
            Assert.True(packages.Any(p => p.Id == package2.Id));
            Assert.True(packages.Any(p => p.Id == package3.Id));
        }
    }
}
