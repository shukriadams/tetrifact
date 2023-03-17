using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class PackageExists : TestBase
    {
        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            Mock<IIndexReadService> indexReadService = new Mock<IIndexReadService>();
            indexReadService
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Returns(true);

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "indexReadService", indexReadService.Object);
            JsonResult result = controller.PackageExists("any-package-id") as JsonResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Unexpected_error()
        {
            Mock<IIndexReadService> indexReadService = new Mock<IIndexReadService>();
            indexReadService
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Throws(new Exception());

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "indexReadService", indexReadService.Object);
            BadRequestObjectResult result = controller.PackageExists("any-package-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
