using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Tetrifact;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;
using System.Linq;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class ListPackages
    {

        [Fact]
        public void Happy_path()
        {
            // inject 3 indices
            Mock<IIndexReadService> mockedIndexReader = new Mock<IIndexReadService>();
            mockedIndexReader
                .Setup(r => r.GetPackageIds(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new string[] { "1", "2", "3" });

            PackagesController controller = NinjectHelper.Get<PackagesController>("indexReadService", mockedIndexReader.Object);

            dynamic json = JsonHelper.ToDynamic(controller.ListPackages(false, 0, 10));
            string[] ids = json.success.packages.ToObject<string[]>();
            Assert.Equal(3, ids.Count());
        }

        [Fact]
        public void Full_package_list()
        {
            Mock<IPackageListService> moqListService = new Mock<IPackageListService>();
            moqListService
                .Setup(r => r.Get(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<Package>() {
                    new Package(), new Package(), new Package() // inject 3 packages
                });

            PackagesController controller = NinjectHelper.Get<PackagesController>("packageListService", moqListService.Object);
            dynamic json = JsonHelper.ToDynamic(controller.ListPackages(true, 0, 10));
            Package[] packages = json.success.packages.ToObject<Package[]>();
            Assert.Equal(3, packages.Count());
        }
    }
}
