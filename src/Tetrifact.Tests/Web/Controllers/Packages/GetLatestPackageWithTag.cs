using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class GetLatestPackageWithTag : TestBase
    {
        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            Mock<IPackageListService> packageListService = new Mock<IPackageListService>();
            packageListService
                .Setup(r => r.GetLatestWithTags(It.IsAny<string[]>()))
                .Returns(new Package());

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "packageListService", packageListService.Object);
            JsonResult result = controller.GetLatestPackageWithTag("any-tag") as JsonResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Unexpected_error()
        {
            Mock<IPackageListService> packageListService = new Mock<IPackageListService>();
            packageListService
                .Setup(r => r.GetLatestWithTags(It.IsAny<string[]>()))
                .Throws(new Exception());

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "packageListService", packageListService.Object);
            BadRequestObjectResult result = controller.GetLatestPackageWithTag("any-tag") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
