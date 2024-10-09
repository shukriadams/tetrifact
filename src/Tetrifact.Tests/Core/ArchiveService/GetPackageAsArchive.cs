using Moq;
using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.ArchiveService
{
    public class GetPackageAsArchive : TestBase
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public async void PackageStreamHasContent()
        {
            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            // create a package
            TestPackage testPackage = PackageHelper.CreateRandomPackage();
            await archiveService.CreateArchive(testPackage.Id);

            // stream package as archive
            using (Stream testContent = archiveService.GetPackageAsArchive(testPackage.Id))
            {
                // ensure that stream contains package content
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent);
                // test package has 1 item
                Assert.Single(items);
                // byte content generated in package should be in archive stream
                Assert.Equal(testPackage.Content, items[testPackage.Path]);
            }
        }

        /// <summary>
        /// Ensures test coverage of code in GetPackageAsArchive() that references an existing archive
        /// file. To do this we repeat the PackageStreamHasContent() test, which generates an archive, then retrieve it again.
        /// 
        /// Coverage test
        /// </summary>
        [Fact]
        public async void GetExistingArchive()
        {
            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            TestPackage testPackage = PackageHelper.CreateRandomPackage();
            await archiveService.CreateArchive(testPackage.Id);

            using (Stream testContent1 = archiveService.GetPackageAsArchive(testPackage.Id))
            using (Stream testContent2 = archiveService.GetPackageAsArchive(testPackage.Id))
            {
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent2);
                Assert.Single(items);
            }
        }

        [Fact]
        public async void GetWithSevenZip()
        {
            Settings.ExternaArchivingExecutable = Path.Combine(Path.GetFullPath($"../../../../"), "lib", "7za.exe");
            Settings.ArchivingMode = ArchivingModes.SevenZip;

            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            TestPackage testPackage = PackageHelper.CreateRandomPackage();
            await archiveService.CreateArchive(testPackage.Id);

            using (Stream testContent1 = archiveService.GetPackageAsArchive(testPackage.Id))
            using (Stream testContent2 = archiveService.GetPackageAsArchive(testPackage.Id))
            {
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent2);
                Assert.Single(items);
            }
        }

        /// <summary>
        /// Archive fetching for a non-existent package should throw expected exception
        /// </summary>
        [Fact]
        public void GetNonExistent()
        {
            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            ArchiveNotFoundException exception = Assert.Throws<ArchiveNotFoundException>(() => {
                using (Stream zipStream = archiveService.GetPackageAsArchive("an invalid package id")){ }
            });
        }

        /// <summary>
        /// Confirms that waiting-for-archive to generate goes into loop, which exits when the archive temp file is replaced with the completed archive.
        /// </summary>
        [Fact]
        public void GetAfterWaiting()
        {
            // create a package 
            TestPackage testPackage = PackageHelper.CreateRandomPackage();
            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            // create a fake archive temp file so GetPackageAsArchive() goes into wait state
            string tempArchivePath = archiveService.GetPackageArchiveTempPath(testPackage.Id);
            File.WriteAllText(tempArchivePath, string.Empty);

            // fake temp archive bypasses archive creation, so make a fake archive too, it needs to be just a file
            string archivePath = archiveService.GetPackageArchivePath(testPackage.Id);
            File.WriteAllText(archivePath, "some-fake-archive-content");

            // lock the temp archive so it doesn't get auto-clobbered (not needed, doesn't work on linux)
            using (Stream lockStream = new FileStream(tempArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // mock IThread.Sleep, in this method, we will release and delete our temp archive lock, so the archive fetch can proceed
                var mockThread = new Mock<IThread>();
                mockThread
                    .Setup(r => r.Sleep(It.IsAny<int>()))
                    .Callback<int>(time => {
                        lockStream.Close();
                        File.Delete(tempArchivePath);
                    });

                // make a custom reader with our mocked Thread
                IArchiveService archiveService2 = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[]{ mockThread.Object });

                using (Stream zipStream = archiveService2.GetPackageAsArchive(testPackage.Id))
                {
                    // confirm we got our fake archive content back
                    Assert.Equal("some-fake-archive-content", StreamsHelper.StreamToString(zipStream));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public async void GetArchiveCompressionEnabled()
        {
            Settings.StorageCompressionEnabled = true;

            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            // create package
            TestPackage testPackage = PackageHelper.CreateRandomPackage();

            // force create archive
            await archiveService.CreateArchive(testPackage.Id);

            using (Stream testContent = archiveService.GetPackageAsArchive(testPackage.Id))
            {
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent);
                Assert.Single(items);
                Assert.Equal(testPackage.Content, items[testPackage.Path]);
            }
        }

        /// <summary>
        /// Tests that trying to build an archive with a missing package file throws expected exception
        /// </summary>
        [Fact]
        public void GetArchive_Nocompress_FileMissing()
        {
            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            TestPackage testPackage = PackageHelper.CreateRandomPackage();

            // delete known package file via disk
            File.Delete(Path.Join(Settings.RepositoryPath, testPackage.Path, testPackage.Hash, "bin"));

            Assert.Throws<ArchiveNotFoundException>(() => { archiveService.GetPackageAsArchive(testPackage.Id); });
        }

        /// <summary>
        /// Tests that trying to build an archive with a missing package file throws expected exception, compression of source files enabled. This
        /// is for coverage.
        /// </summary>
        [Fact]
        public void GetArchive_CompressEnabled_FileMissing()
        {
            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            Settings.StorageCompressionEnabled = true;

            TestPackage testPackage = PackageHelper.CreateRandomPackage();

            // delete known package file via disk
            File.Delete(Path.Join(Settings.RepositoryPath, testPackage.Path, testPackage.Hash, "bin"));

            Assert.Throws<ArchiveNotFoundException>(() => { archiveService.GetPackageAsArchive(testPackage.Id); });
        }
    }
}
