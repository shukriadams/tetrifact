using Moq;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.ArchiveService
{
    public class GetPackageAsArchive
    {
        MoqHelper MoqHelper { get; set; } = new MoqHelper(new TestContext());

        /// <summary>
        /// Esnure that package can be streamed.
        /// </summary>
        [Fact]
        public void PackageStreamHappyPath()
        {
            // force package archive to exist on filesystem
            Mock<IFileSystem> filesystem = new Mock<IFileSystem>();
            filesystem
                .Setup(r => r.File.Exists(It.IsAny<string>()))
                .Returns(true);

            // force package stream to be some string
            Mock<IFileStreamProvider> fileStreamProvider = new Mock<IFileStreamProvider>();
            fileStreamProvider
                .Setup(r => r.Read(It.IsAny<string>()))
                .Returns(StreamsHelper.StreamFromString("my-value"));

            Core.ArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { filesystem, fileStreamProvider });

            // get package as stream
            using (Stream testStream = archiveService.GetPackageAsArchive("some-package-id"))
            {
                // must get back fake stream content we put it
                Assert.Equal("my-value", StreamsHelper.StreamToString(testStream));
            }
        }


        /// <summary>
        /// Archive fetching for a non-existent package should throw expected exception
        /// </summary>
        [Fact]
        public void GetNonExistent()
        {
            // force package archive to not exist on filesystem
            Mock<IFileSystem> filesystem = new Mock<IFileSystem>();
            filesystem
                .Setup(r => r.File.Exists(It.IsAny<string>()))
                .Returns(false);

            Core.ArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { filesystem });

            ArchiveNotFoundException exception = Assert.Throws<ArchiveNotFoundException>(() => {
                using (Stream zipStream = archiveService.GetPackageAsArchive("some-id")){ }
            });
        }
    }
}
