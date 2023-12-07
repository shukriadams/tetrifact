using Microsoft.AspNetCore.Mvc;
using Moq;
using Tetrifact.Core;
using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Controllers
{
    public class GetItem : TestBase
    {
        [Fact]
        public void Happy_path()
        {
            Mock<IIndexReadService> repoCleanServiceMock = new Mock<IIndexReadService>();
            repoCleanServiceMock
                .Setup(r => r.GetFile(It.IsAny<string>()))
                .Returns(new GetFileResponse(StreamsHelper.StreamFromString("some-content"), "some-file"));

            W.FilesController controller = NinjectHelper.Get<W.FilesController>(base.Settings, "indexService", repoCleanServiceMock.Object);
            FileStreamResult result = controller.GetItem("any-id") as FileStreamResult;
            Assert.Equal("some-content", StreamsHelper.StreamToString(result.FileStream));
        }

        [Fact]
        public void File_not_found()
        {
            // return null
            Mock<IIndexReadService> repoCleanServiceMock = new Mock<IIndexReadService>();
            repoCleanServiceMock
                .Setup(r => r.GetFile(It.IsAny<string>()));

            W.FilesController controller = NinjectHelper.Get<W.FilesController>(base.Settings, "indexService", repoCleanServiceMock.Object);
            NotFoundObjectResult result = controller.GetItem("any-id") as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Invalid_file()
        {
            // return null stream as file content
            Mock<IIndexReadService> repoCleanServiceMock = new Mock<IIndexReadService>();
            repoCleanServiceMock
                .Setup(r => r.GetFile(It.IsAny<string>()))
                .Returns(new GetFileResponse(null, "some-file"));

            W.FilesController controller = NinjectHelper.Get<W.FilesController>(base.Settings, "indexService", repoCleanServiceMock.Object);
            BadRequestObjectResult result = controller.GetItem("any-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Invalid_identifier()
        {
            Mock<IIndexReadService> repoCleanServiceMock = new Mock<IIndexReadService>();
            // force exception for invalid id
            repoCleanServiceMock
                .Setup(r => r.GetFile(It.IsAny<string>()))
                .Throws(new InvalidFileIdentifierException(""));

            W.FilesController controller = NinjectHelper.Get<W.FilesController>(base.Settings, "indexService", repoCleanServiceMock.Object);
            BadRequestObjectResult result = controller.GetItem("any-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
