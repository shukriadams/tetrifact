using Microsoft.AspNetCore.Mvc;
using Moq;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class SetPackageCreateDate : TestBase
    {
        [Fact]
        public void Happy_path()
        {
            Mock<IIndexReadService> indexReadService = new Mock<IIndexReadService>();
            indexReadService
                .Setup(r => r.UpdatePackageCreateDate(It.IsAny<string>(), It.IsAny<string>()));

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "indexReadService", indexReadService.Object);
            JsonResult result = controller.SetPackageCreateDate("somePackage", "some-date") as JsonResult;
            Assert.NotNull(result);
        }

    }
}
