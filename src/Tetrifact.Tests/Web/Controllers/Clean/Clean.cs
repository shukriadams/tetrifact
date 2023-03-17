using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Controllers.Clean
{
    public class Clean 
    {
        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Touch()
        {
            Mock<IRepositoryCleanService> repoCleanServiceMock = new Mock<IRepositoryCleanService>();
            repoCleanServiceMock
                .Setup(r => r.Clean())
                .Returns(new CleanResult());

            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.PurgeOldArchives());

            CleanController controller = NinjectHelper.Get<CleanController>(null, "repositoryCleaner", repoCleanServiceMock.Object, "archiveService", archiveServiceMock.Object);
            JsonResult result = controller.Clean() as JsonResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Handle_500()
        {
            // mock first service hit only
            Mock<IRepositoryCleanService> repoCleanServiceMock = new Mock<IRepositoryCleanService>();
            repoCleanServiceMock
                .Setup(r => r.Clean())
                .Throws(new Exception("unexpected error"));

            CleanController controller = NinjectHelper.Get<CleanController>(null, "repositoryCleaner", repoCleanServiceMock.Object);
            BadRequestObjectResult result = controller.Clean() as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
