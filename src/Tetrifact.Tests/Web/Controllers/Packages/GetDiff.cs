using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class GetPackagesDiff
    {
        private TestContext _testContext = new TestContext();

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            Mock<IPackageDiffService> packageDiffService = new Mock<IPackageDiffService>();
            packageDiffService
                .Setup(r => r.GetDifference(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Tetrifact.Core.PackageDiff());

            PackagesController controller = _testContext.Get<PackagesController>("packageDiffService", packageDiffService.Object);
            JsonResult result = controller.GetDiff("upstream", "downstream") as JsonResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Package_not_found()
        {
            Mock<IPackageDiffService> packageDiffService = new Mock<IPackageDiffService>();
            packageDiffService
                .Setup(r => r.GetDifference(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PackageNotFoundException("package-id"));

            PackagesController controller = _testContext.Get<PackagesController>("packageDiffService", packageDiffService.Object);
            NotFoundObjectResult result = controller.GetDiff("upstream", "downstream") as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Unexpected_error()
        {
            Mock<IPackageDiffService> packageDiffService = new Mock<IPackageDiffService>();
            packageDiffService
                .Setup(r => r.GetDifference(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            PackagesController controller = _testContext.Get<PackagesController>("packageDiffService", packageDiffService.Object);
            BadRequestObjectResult result = controller.GetDiff("upstream", "downstream") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
