using Xunit;
using Tetrifact.Web;
using Moq;
using Tetrifact.Core;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Tetrifact.Tests.Controllers.Archives
{
    public class GetArchive: TestBase
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageAsArchive(It.IsAny<string>()))
                .Returns(StreamsHelper.StreamFromString("abc"));

            ArchivesController controller = NinjectHelper.Get<ArchivesController>(base.Settings, "archiveService", archiveServiceMock.Object);

            FileStreamResult result = controller.GetArchive("any-package-id") as FileStreamResult;
            Assert.NotNull(result);
            Assert.Equal("abc", StreamsHelper.StreamToString(result.FileStream));
        }

        [Fact]
        public void Handle_404()
        {
            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageAsArchive(It.IsAny<string>()))
                .Callback(()=>{ 
                    throw new PackageNotFoundException("123");
                });

            ArchivesController controller = NinjectHelper.Get<ArchivesController>(base.Settings, "archiveService", archiveServiceMock.Object);
            NotFoundObjectResult result = controller.GetArchive("any-package-id") as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Handle_500()
        {
            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageAsArchive(It.IsAny<string>()))
                .Callback(() => {
                    throw new Exception("123");
                });

            ArchivesController controller = NinjectHelper.Get<ArchivesController>(base.Settings, "archiveService", archiveServiceMock.Object);
            BadRequestObjectResult result = controller.GetArchive("any-package-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
