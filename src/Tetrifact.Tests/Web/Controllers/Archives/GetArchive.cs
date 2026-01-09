using Xunit;
using Tetrifact.Web;
using Moq;
using Tetrifact.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO.Abstractions;

namespace Tetrifact.Tests.Controllers.Archives
{
    public class GetArchive
    {
        private TestContext _testContext = new TestContext();

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            // pathway stub : all packages exist
            Mock<IIndexReadService> indexReaderMock = new Mock<IIndexReadService>();
            indexReaderMock
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Returns(true);

            // pathway stub : all package archives exist
            Mock<IFileSystem> filesystem = new Mock<IFileSystem>();
            filesystem
                .Setup(r => r.File.Exists(It.IsAny<string>()))
                .Returns(true);
            
            // pathway stub : archive lookup returns a stream with known content
            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageAsArchive(It.IsAny<string>()))
                .Returns(StreamsHelper.StreamFromString("abc"));

            ArchivesController controller = _testContext.Instantiate<ArchivesController>("archiveService", archiveServiceMock.Object, "indexReader", indexReaderMock.Object, "fileSystem", filesystem.Object);
            HttpHelper.EnsureContext(controller);

            FileStreamResult result = controller.GetArchive("any-package-id", "my-waiver") as FileStreamResult;
            Assert.NotNull(result);
            // confirm known content in stub
            Assert.Equal("abc", StreamsHelper.StreamToString(result.FileStream));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="PackageNotFoundException"></exception>
        [Fact]
        public void Handle_404()
        {
            Mock<IArchiveService> archiveServiceMock = new Mock<IArchiveService>();
            archiveServiceMock
                .Setup(r => r.GetPackageAsArchive(It.IsAny<string>()))
                .Callback(()=>{ 
                    throw new PackageNotFoundException("123");
                });

            ArchivesController controller = _testContext.Instantiate<ArchivesController>("archiveService", archiveServiceMock.Object);
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

            ArchivesController controller = _testContext.Instantiate<ArchivesController>("archiveService", archiveServiceMock.Object, "indexReader", indexReaderMock.Object);
            BadRequestObjectResult result = controller.GetArchive("any-package-id", "my-waiver") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
