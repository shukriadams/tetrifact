using Moq;
using System.Collections.Generic;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;
using System.Linq;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class ListPackages : TestBase
    {
        [Fact]
        public void Package_Id_List()
        {
            // inject 3 indices
            Mock<IPackageListService> mockedPackageListService = new Mock<IPackageListService>();
            mockedPackageListService
                .Setup(r => r.Get(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new Package[] { 
                        new Package { Id = "1" }, 
                        new Package { Id = "2" }, 
                        new Package { Id = "3" } 
                    });

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "packageListService", mockedPackageListService.Object);

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

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "packageListService", moqListService.Object);
            dynamic json = JsonHelper.ToDynamic(controller.ListPackages(true, 0, 10));
            Package[] packages = json.success.packages.ToObject<Package[]>();
            Assert.Equal(3, packages.Count());
        }
    }
}
