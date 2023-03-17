using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class VerifyPackage : TestBase
    {
        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            Mock<IIndexReadService> indexReadService = new Mock<IIndexReadService>();
            indexReadService
                .Setup(r => r.VerifyPackage(It.IsAny<string>()))
                .Returns((true, "some-description"));

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "indexReadService", indexReadService.Object);
            JsonResult result = controller.VerifyPackage("any-package-id") as JsonResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Package_not_found()
        {
            Mock<IIndexReadService> indexReadService = new Mock<IIndexReadService>();
            indexReadService
                .Setup(r => r.VerifyPackage(It.IsAny<string>()))
                .Throws(new PackageNotFoundException("package-id"));

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "indexReadService", indexReadService.Object);
            NotFoundObjectResult result = controller.VerifyPackage("any-package-id") as NotFoundObjectResult;
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
                .Setup(r => r.VerifyPackage(It.IsAny<string>()))
                .Throws(new Exception());

            PackagesController controller = NinjectHelper.Get<PackagesController>(this.Settings, "indexReadService", indexReadService.Object);
            BadRequestObjectResult result = controller.VerifyPackage("any-package-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
