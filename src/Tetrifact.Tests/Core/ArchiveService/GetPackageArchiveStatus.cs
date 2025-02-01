using Moq;
using System;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.ArchiveService
{
    public class GetPackageArchiveStatus 
    {
        MoqHelper MoqHelper { get; set; } = new MoqHelper(new TestContext());

        /// <summary>
        /// Requesting an invalid package should return package not found
        /// </summary>
        [Fact]
        public void GetNonExistent()
        {
            // set up
            IArchiveService archiveService = MoqHelper.CreateInstanceWithAllMoqed<Core.ArchiveService>();

            // do
            ArchiveProgressInfo progress = archiveService.GetPackageArchiveStatus("some-invalid-package-id");

            // test
            Assert.Equal(PackageArchiveCreationStates.Processed_PackageNotFound, progress.State);
        }

        /// <summary>
        /// Archive create status should be 0 (starting) when neither archive exists, nor archive creation has started yet
        /// </summary>
        [Fact]
        public void GetArchiveStarted()
        {
            // force any package to exist in package service
            Mock<IIndexReadService> indexReader = new Mock<IIndexReadService>();
            indexReader
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Returns(true);

            // force any package archive to not exist in filesystem
            Mock<IFileSystem> filesystem = new Mock<IFileSystem>();
            filesystem
                .Setup(r => r.File.Exists(It.IsAny<string>()))
                .Returns(false);

            // do
            IArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { indexReader, filesystem });

            // test
            Assert.Equal(PackageArchiveCreationStates.Processed_ArchiveNotGenerated, archiveService.GetPackageArchiveStatus("any-package-id").State);
        }

        /// <summary>
        /// Archive create status should be "already in progress".
        /// </summary>
        [Fact]
        public void GetArchiveQueued()
        {
            // setup
            string archivePath = string.Empty;
            string archiveQueuePath = string.Empty;

            // force package to exist
            Mock<IIndexReadService> indexReader = new Mock<IIndexReadService>();
            indexReader
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Returns(true);

            Mock<IFileSystem> filesystem = new Mock<IFileSystem>();
            filesystem
                .Setup(r => r.File.Exists(It.IsAny<string>()))
                .Returns((string path) => {
                    // force archive to not exist
                    if (path == archivePath)
                        return false;
                    
                    // force archive queue to exist
                    if (path == archiveQueuePath)
                        return true;

                    throw new Exception($"Unexpected path {path} received");
                });

            IArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { indexReader, filesystem });
            
            // for filesystem forces above, get the path each item is expected to be at
            archivePath = archiveService.GetPackageArchivePath("some-package");
            archiveQueuePath = archiveService.GetPackageArchiveQueuePath("some-package");

            // do
            PackageArchiveCreationStates status = archiveService.GetPackageArchiveStatus("some-package").State;

            // tests
            Assert.Equal(PackageArchiveCreationStates.Queued, status);
        }

        /// <summary>
        /// Archive status should be complete.
        /// </summary>
        [Fact]
        public void GetArchiveComplete()
        {
            // setup
            string archivePath = string.Empty;

            // force package to exist
            Mock<IIndexReadService> indexReader = new Mock<IIndexReadService>();
            indexReader
                .Setup(r => r.PackageExists(It.IsAny<string>()))
                .Returns(true);

            Mock<IFileSystem> filesystem = new Mock<IFileSystem>();
            filesystem
                .Setup(r => r.File.Exists(It.IsAny<string>()))
                .Returns((string path) => {
                    // force archive to exist
                    if (path == archivePath)
                        return true;

                    throw new Exception($"Unexpected path {path} received");
                });

            IArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { indexReader, filesystem });

            // for filesystem forces above, get the path each item is expected to be at
            archivePath = archiveService.GetPackageArchivePath("some-package");

            // do
            PackageArchiveCreationStates status = archiveService.GetPackageArchiveStatus("some-package").State;

            // tests
            Assert.Equal(PackageArchiveCreationStates.Processed_ArchiveAvailable, status);
        }
    }
}
