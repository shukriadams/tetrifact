using Xunit;
using Tetrifact.Web;
using Moq;
using Tetrifact.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO.Abstractions;

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
            // stub out method so lookup passes
            Mock<IIndexReadService> indexReaderMock = new Mock<IIndexReadService>();
            indexReaderMock
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Returns(true);

            // stub out filesystem lookup so archive appears to exist
            Mock<IFileSystem> filesystem = new Mock<IFileSystem>();
            filesystem
                .Setup(r => r.File.Exists(It.IsAny<string>()))
                .Returns(true);

            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageAsArchive(It.IsAny<string>()))
                .Returns(StreamsHelper.StreamFromString("abc"));

            ArchivesController controller = TestContext.Get<ArchivesController>("archiveService", archiveServiceMock.Object, "indexReader", indexReaderMock.Object, "fileSystem", filesystem.Object);
            HttpHelper.EnsureContext(controller);

            FileStreamResult result = controller.GetArchive("any-package-id", "my-waiver") as FileStreamResult;
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

            ArchivesController controller = TestContext.Get<ArchivesController>("archiveService", archiveServiceMock.Object);
            NotFoundObjectResult result = controller.GetArchive("any-package-id", "my-waiver") as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Handle_500()
        {
            // stub out method so lookup passes
            Mock<IIndexReadService> indexReaderMock = new Mock<IIndexReadService>();
            indexReaderMock
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Returns(true);

            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageArchivePath(It.IsAny<string>()))
                .Callback(() => {
                    throw new Exception("123");
                });

            ArchivesController controller = TestContext.Get<ArchivesController>("archiveService", archiveServiceMock.Object, "indexReader", indexReaderMock.Object);
            BadRequestObjectResult result = controller.GetArchive("any-package-id", "my-waiver") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
