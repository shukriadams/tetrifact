using System.Collections.Generic;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class ListAvailablePackages
    {
        private TestContext _testContext = new TestContext();

        private PackageHelper _packageHelper;

        public ListAvailablePackages()
        {
            _packageHelper = new PackageHelper(_testContext);
        }
        
        [Fact]
        public void HappyPath()
        {
            TestPackage package1 = _packageHelper.CreateRandomPackage();
            TestPackage package2 = _packageHelper.CreateRandomPackage();
            TestPackage package3 = _packageHelper.CreateRandomPackage();

            IPackageListService listService = _testContext.Get<IPackageListService>();
            IEnumerable<Package> packages = listService.Get(0, 10);
            
            Assert.Equal(3, packages.Count());
            Assert.True(packages.Any(p => p.Id == package1.Id));
            Assert.True(packages.Any(p => p.Id == package2.Id));
            Assert.True(packages.Any(p => p.Id == package3.Id));
        }
    }
}
