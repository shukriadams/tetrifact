using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Controllers.Archives
{
    public class GetArchiveStatus
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            ArchivesController controller = NinjectHelper.Get<ArchivesController>(null);
            dynamic result = JsonHelper.ToDynamic(controller.GetArchiveStatus("invalid-package"));
            Assert.Null(result.success);
            Assert.NotNull(result.error);
        }

        [Fact]
        public void Handle_404()
        {
            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageArchiveStatus(It.IsAny<string>()))
                .Throws(new PackageNotFoundException("123"));

            ArchivesController controller = NinjectHelper.Get<ArchivesController>(null, "archiveService", archiveServiceMock.Object);
            NotFoundObjectResult result = controller.GetArchiveStatus("any-package-id") as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Handle_500()
        {
            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageArchiveStatus(It.IsAny<string>()))
                .Throws(new Exception("123"));

            ArchivesController controller = NinjectHelper.Get<ArchivesController>(null,"archiveService", archiveServiceMock.Object);
            BadRequestObjectResult result = controller.GetArchiveStatus("any-package-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
