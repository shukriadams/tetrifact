using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class GetPackage : TestBase
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            Mock<IIndexReadService> indexReadService = new Mock<IIndexReadService>();
            indexReadService
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Returns(new Manifest());

            PackagesController controller = NinjectHelper.Get<PackagesController>(base.Settings, "indexReadService", indexReadService.Object);
            JsonResult result = controller.GetPackage("any-package-id") as JsonResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Package_not_found()
        {
            Mock<IIndexReadService> indexReadService = new Mock<IIndexReadService>();
            indexReadService
                .Setup(r => r.GetManifest(It.IsAny<string>())); // return null

            PackagesController controller = NinjectHelper.Get<PackagesController>(base.Settings, "indexReadService", indexReadService.Object);
            NotFoundObjectResult result = controller.GetPackage("any-package-id") as NotFoundObjectResult;
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
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Throws(new Exception());

            PackagesController controller = NinjectHelper.Get<PackagesController>(base.Settings, "indexReadService", indexReadService.Object);
            BadRequestObjectResult result = controller.GetPackage("any-package-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
