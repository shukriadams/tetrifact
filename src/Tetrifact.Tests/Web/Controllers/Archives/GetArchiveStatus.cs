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
        private TestContext _testContext = new TestContext();

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            ArchivesController controller = _testContext.Get<ArchivesController>();
            dynamic result = JsonHelper.ToDynamic(controller.GetArchiveStatus("invalid-package"));
            Assert.Equal(result.success.status.State.ToString(), ((int)PackageArchiveCreationStates.Processed_PackageNotFound).ToString());
        }

        [Fact]
        public void Handle_404()
        {
            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageArchiveStatus(It.IsAny<string>()))
                .Throws(new PackageNotFoundException("123"));

            ArchivesController controller = _testContext.Get<ArchivesController>("archiveService", archiveServiceMock.Object);
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

            ArchivesController controller = _testContext.Get<ArchivesController>("archiveService", archiveServiceMock.Object);
            BadRequestObjectResult result = controller.GetArchiveStatus("any-package-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
