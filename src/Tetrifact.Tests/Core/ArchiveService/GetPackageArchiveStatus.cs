using Moq;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.ArchiveService
{
    public class GetPackageArchiveStatus : FileSystemBase
    {
        /// <summary>
        /// Requesting an invalid package should throw proper exception
        /// </summary>
        [Fact]
        public void GetNonExistent()
        {
            // mock a non-existent package
            Mock<IIndexReadService> indexReader = new Mock<IIndexReadService>();
            indexReader
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Returns(false);

            IArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[]{ base.Settings, indexReader });
            Assert.Throws<PackageNotFoundException>(()=> archiveService.GetPackageArchiveStatus("invalid-id"));
        }

        /// <summary>
        /// Archive create status should be 0 (starting) when neither archive exists, nor archive creation has started yet
        /// </summary>
        [Fact]
        public void GetArchiveStarted()
        {
            // force package exists
            Mock<IIndexReadService> indexReader = new Mock<IIndexReadService>();
            indexReader
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Returns(true);

            // force filesystem to return false for all file checks (ie, archive + archive temp)
            Mock<IFileSystem> filesystem = new Mock<IFileSystem>();
            filesystem
                .Setup(r => r.File.Exists(It.IsAny<string>()))
                .Returns(false);

            IArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { base.Settings, indexReader, filesystem });
            Assert.Equal(0, archiveService.GetPackageArchiveStatus("any-package-id"));
        }

        /// <summary>
        /// Archive create status should be "already in progress".
        /// </summary>
        [Fact]
        public void GetArchiveInProgress()
        {
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);
            
            // mock temp archive (temp = in progress)
            File.WriteAllText(this.ArchiveService.GetPackageArchiveTempPath(testPackage.Id), string.Empty);

            Assert.Equal(1, this.ArchiveService.GetPackageArchiveStatus(testPackage.Id));
        }

        /// <summary>
        /// Archive status should be complete.
        /// </summary>
        [Fact]
        public void GetArchiveComplete()
        {
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

            // mock existing archive file
            File.WriteAllText(this.ArchiveService.GetPackageArchivePath(testPackage.Id), string.Empty);

            Assert.Equal(2, this.ArchiveService.GetPackageArchiveStatus(testPackage.Id));
        }
    }
}
