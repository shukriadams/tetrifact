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
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

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
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

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
            this.Settings.SevenZipBinaryPath = Path.Combine(Path.GetFullPath($"../../../../"), "packages", "7z.libs", "23.1.0", "bin", "x64", "7z.dll"); 

            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

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
        /// Confirms that a timeout exception is thrown if package creation takes too long.
        /// </summary>
        [Fact/* (Skip = "locking file doesn't work on linux, find cross-platform solution")*/]
        public void GetTimesOut()
        {
            // zero wait times to speed up test, this should trigger an instant timeout
            base.Settings.ArchiveWaitTimeout = 1; // second
            base.Settings.ArchiveAvailablePollInterval = 0; // no poll interval, so reads instantly

            // we need a valid package first
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

            // lock the temp archive file in the system, this will block creating a new archive
            LockProvider.Instance.Lock(ProcessLockCategories.Archive_Create, ArchiveService.GetPackageArchiveTempPath(testPackage.Id));

            Assert.Throws<TimeoutException>(() => {
                using (Stream zipStream = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
                {
                    // do nothing, exception expected
                }
            });
        }

        /// <summary>
        /// Confirms that waiting-for-archive to generate goes into loop, which exits when the archive temp file is replaced with the completed archive.
        /// </summary>
        [Fact]
        public void GetAfterWaiting()
        {
            // create a package 
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

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
                IArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[]{mockThread.Object, this.Settings });

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
            base.Settings.IsStorageCompressionEnabled = true;

            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);
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
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

            // delete known package file via disk
            File.Delete(Path.Join(this.Settings.RepositoryPath, testPackage.Path, testPackage.Hash, "bin"));

            Exception ex = Assert.Throws<Exception>(() => { this.ArchiveService.GetPackageAsArchive(testPackage.Id); });
            Assert.Contains("Failed to find expected package file", ex.Message);
        }

        /// <summary>
        /// Tests that trying to build an archive with a missing package file throws expected exception, compression of source files enabled. This
        /// is for coverage.
        /// </summary>
        [Fact]
        public void GetArchive_CompressEnabled_FileMissing()
        {
            base.Settings.IsStorageCompressionEnabled = true;

            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

            // delete known package file via disk
            File.Delete(Path.Join(this.Settings.RepositoryPath, testPackage.Path, testPackage.Hash, "bin"));

            Exception ex = Assert.Throws<Exception>(() => { this.ArchiveService.GetPackageAsArchive(testPackage.Id); });
            Assert.Contains("Failed to find expected package file", ex.Message);
        }

        /// <summary>
        /// Ensure graceful handling of locked temp file from previous archive generating attempt
        /// </summary>
        [Fact]
        public void GetArchive_Preexisting_locked_tempFile()
        {
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

            // lock archive
            LockProvider.Instance.Lock(ProcessLockCategories.Archive_Create, ArchiveService.GetPackageArchiveTempPath(testPackage.Id));
            
            Settings.ArchiveWaitTimeout = 0;

            // attempt to start a new archive generation, this should exit immediately
            TimeoutException ex = Assert.Throws<TimeoutException>(() => { this.ArchiveService.GetPackageAsArchive(testPackage.Id); });
            Assert.NotNull(ex);
            Assert.True(ArchiveLogger.ContainsFragment("skipped, existing process detected"));
        }
    }
}
