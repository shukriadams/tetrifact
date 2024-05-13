using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.ArchiveService
{
    public class GetPackageAsArchive : FileSystemBase
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void PackageStreamHasContent()
        {
            // create a package
            TestPackage testPackage = PackageHelper.CreateRandomPackage(SettingsHelper.CurrentSettingsContext);

            // stream package as archive
            using (Stream testContent = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
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
        public void GetExistingArchive()
        {
            TestPackage testPackage = PackageHelper.CreateRandomPackage(SettingsHelper.CurrentSettingsContext);

            using (Stream testContent1 = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
            using (Stream testContent2 = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
            {
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent2);
                Assert.Single(items);
            }
        }

        [Fact]
        public void GetWithSevenZip()
        {
            SettingsHelper.CurrentSettingsContext.SevenZipBinaryPath = Path.Combine(Path.GetFullPath($"../../../../"), "packages", "7z.libs", "23.1.0", "bin", "x64", "7z.dll"); 

            TestPackage testPackage = PackageHelper.CreateRandomPackage(SettingsHelper.CurrentSettingsContext);

            using (Stream testContent1 = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
            using (Stream testContent2 = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
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
            PackageNotFoundException exception = Assert.Throws<PackageNotFoundException>(() => {
                using (Stream zipStream = this.ArchiveService.GetPackageAsArchive("my invalid id")){ }
            });

            Assert.Equal("my invalid id", exception.PackageId);
        }

        /// <summary>
        /// Confirms that waiting-for-archive to generate goes into loop, which exits when the archive temp file is replaced with the completed archive.
        /// </summary>
        [Fact]
        public void GetAfterWaiting()
        {
            // create a package 
            TestPackage testPackage = PackageHelper.CreateRandomPackage(SettingsHelper.CurrentSettingsContext);

            // create a fake archive temp file so GetPackageAsArchive() goes into wait state
            string tempArchivePath = this.ArchiveService.GetPackageArchiveTempPath(testPackage.Id);
            File.WriteAllText(tempArchivePath, string.Empty);

            // fake temp archive bypasses archive creation, so make a fake archive too, it needs to be just a file
            string archivePath = this.ArchiveService.GetPackageArchivePath(testPackage.Id);
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
                IArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[]{mockThread.Object, SettingsHelper.CurrentSettingsContext });

                using (Stream zipStream = archiveService.GetPackageAsArchive(testPackage.Id))
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
        public void GetArchiveCompressionEnabled()
        {
            SettingsHelper.CurrentSettingsContext.IsStorageCompressionEnabled = true;
        
            // create package
            TestPackage testPackage = PackageHelper.CreateRandomPackage(SettingsHelper.CurrentSettingsContext);

            // force create archive
            this.ArchiveService.CreateArchive(testPackage.Id);

            using (Stream testContent = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
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
            TestPackage testPackage = PackageHelper.CreateRandomPackage(SettingsHelper.CurrentSettingsContext);

            // delete known package file via disk
            File.Delete(Path.Join(SettingsHelper.CurrentSettingsContext.RepositoryPath, testPackage.Path, testPackage.Hash, "bin"));

            Assert.Throws<ArchiveNotFoundException>(() => { this.ArchiveService.GetPackageAsArchive(testPackage.Id); });
        }

        /// <summary>
        /// Tests that trying to build an archive with a missing package file throws expected exception, compression of source files enabled. This
        /// is for coverage.
        /// </summary>
        [Fact]
        public void GetArchive_CompressEnabled_FileMissing()
        {
            SettingsHelper.CurrentSettingsContext.IsStorageCompressionEnabled = true;

            TestPackage testPackage = PackageHelper.CreateRandomPackage(SettingsHelper.CurrentSettingsContext);

            // delete known package file via disk
            File.Delete(Path.Join(SettingsHelper.CurrentSettingsContext.RepositoryPath, testPackage.Path, testPackage.Hash, "bin"));

            Assert.Throws<ArchiveNotFoundException>(() => { this.ArchiveService.GetPackageAsArchive(testPackage.Id); });
        }
    }
}
