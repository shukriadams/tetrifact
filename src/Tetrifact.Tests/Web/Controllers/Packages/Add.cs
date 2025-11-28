using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Packages
{
    public class AddPackage
    {
        private TestContext _testContext = new TestContext();

        [Fact]
        public void Happy_path()
        {
            Mock<IPackageCreateService> packageCreateService = new Mock<IPackageCreateService>();
            packageCreateService
                .Setup(r => r.Create(It.IsAny<PackageCreateArguments>()))
                .Returns(new PackageCreateResult { Success = true });

            PackagesController controller = _testContext.Get<PackagesController>("packageCreateService", packageCreateService.Object);
            JsonResult result = controller.Add(new PackageCreateFromPost{ }) as JsonResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// Coverage
        /// </summary>
        [Fact]
        public void Insufficient_space_error()
        {
            Mock<IIndexReadService> indexReadService = new Mock<IIndexReadService>();
            indexReadService
                .Setup(r => r.GetDiskUseSats())
                .Returns(new DiskUseStats{ });

            PackagesController controller = _testContext.Get<PackagesController>("indexReadService", indexReadService.Object);
            BadRequestObjectResult result = controller.Add(new PackageCreateFromPost { }) as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Invalid_archive_format()
        {
            Mock<IPackageCreateService> packageCreateService = new Mock<IPackageCreateService>();
            packageCreateService
                .Setup(r => r.Create(It.IsAny<PackageCreateArguments>()))
                .Returns(new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidArchiveFormat });

            PackagesController controller = _testContext.Get<PackagesController>("packageCreateService", packageCreateService.Object);
            BadRequestObjectResult result = controller.Add(new PackageCreateFromPost { }) as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Invalid_file_count()
        {
            Mock<IPackageCreateService> packageCreateService = new Mock<IPackageCreateService>();
            packageCreateService
                .Setup(r => r.Create(It.IsAny<PackageCreateArguments>()))
                .Returns(new PackageCreateResult { ErrorType = PackageCreateErrorTypes.InvalidFileCount });

            PackagesController controller = _testContext.Get<PackagesController>("packageCreateService", packageCreateService.Object);
            BadRequestObjectResult result = controller.Add(new PackageCreateFromPost { }) as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Package_exists()
        {
            Mock<IPackageCreateService> packageCreateService = new Mock<IPackageCreateService>();
            packageCreateService
                .Setup(r => r.Create(It.IsAny<PackageCreateArguments>()))
                .Returns(new PackageCreateResult { ErrorType = PackageCreateErrorTypes.PackageExists });

            PackagesController controller = _testContext.Get<PackagesController>("packageCreateService", packageCreateService.Object);
            BadRequestObjectResult result = controller.Add(new PackageCreateFromPost { }) as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Missing_value()
        {
            Mock<IPackageCreateService> packageCreateService = new Mock<IPackageCreateService>();
            packageCreateService
                .Setup(r => r.Create(It.IsAny<PackageCreateArguments>()))
                .Returns(new PackageCreateResult { ErrorType = PackageCreateErrorTypes.MissingValue });

            PackagesController controller = _testContext.Get<PackagesController>("packageCreateService", packageCreateService.Object);
            BadRequestObjectResult result = controller.Add(new PackageCreateFromPost { }) as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Unexpected_error()
        {
            Mock<IPackageCreateService> packageCreateService = new Mock<IPackageCreateService>();
            packageCreateService
                .Setup(r => r.Create(It.IsAny<PackageCreateArguments>()))
                .Returns(new PackageCreateResult { ErrorType = PackageCreateErrorTypes.UnexpectedError });

            PackagesController controller = _testContext.Get<PackagesController>("packageCreateService", packageCreateService.Object);
            BadRequestObjectResult result = controller.Add(new PackageCreateFromPost { }) as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Unexpected_thrown_exception()
        {
            Mock<IPackageCreateService> packageCreateService = new Mock<IPackageCreateService>();
            packageCreateService
                .Setup(r => r.Create(It.IsAny<PackageCreateArguments>()))
                .Throws(new Exception());

            PackagesController controller = _testContext.Get<PackagesController>("packageCreateService", packageCreateService.Object);
            BadRequestObjectResult result = controller.Add(new PackageCreateFromPost { }) as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
