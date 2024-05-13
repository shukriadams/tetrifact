using Moq;
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

            IArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[]{ SettingsHelper.CurrentSettingsContext, indexReader });
            ArchiveProgressInfo progress = archiveService.GetPackageArchiveStatus("invalid-id");
            Assert.Equal(PackageArchiveCreationStates.Processed_PackageNotFound, progress.State);
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

            IArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { SettingsHelper.CurrentSettingsContext, indexReader, filesystem });
            Assert.Equal(PackageArchiveCreationStates.Processed_ArchiveNotAvailableNotGenerated, archiveService.GetPackageArchiveStatus("any-package-id").State);
        }

        /// <summary>
        /// Archive create status should be "already in progress".
        /// </summary>
        [Fact]
        public void GetArchiveQueued()
        {
            Core.ArchiveService archiveService = NinjectHelper.Get<Core.ArchiveService>();

            // create package, mock existing archive file
            TestPackage randomPackage = PackageHelper.CreateRandomPackage();
            archiveService.QueueArchiveCreation(randomPackage.Id);

            Assert.Equal(PackageArchiveCreationStates.Queued, archiveService.GetPackageArchiveStatus(randomPackage.Id).State);
        }

        /// <summary>
        /// Archive status should be complete.
        /// </summary>
        [Fact]
        public void GetArchiveComplete()
        {
            Core.ArchiveService archiveService = NinjectHelper.Get<Core.ArchiveService>();

            // create package, mock existing archive file
            TestPackage randomPackage = PackageHelper.CreateRandomPackage();
            PackageHelper.FakeArchiveOnDisk(randomPackage);

            Assert.Equal(PackageArchiveCreationStates.Processed_ArchiveAvailable, archiveService.GetPackageArchiveStatus(randomPackage.Id).State);
        }
    }
}
