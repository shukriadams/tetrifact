using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class Get : TestBase
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

            PackagesController controller = TestContext.Get<PackagesController>("indexReadService", indexReadService.Object);
            JsonResult result = controller.Get("any-package-id") as JsonResult;
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

            PackagesController controller = TestContext.Get<PackagesController>("indexReadService", indexReadService.Object);
            NotFoundObjectResult result = controller.Get("any-package-id") as NotFoundObjectResult;
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

            PackagesController controller = TestContext.Get<PackagesController>("indexReadService", indexReadService.Object);
            BadRequestObjectResult result = controller.Get("any-package-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
