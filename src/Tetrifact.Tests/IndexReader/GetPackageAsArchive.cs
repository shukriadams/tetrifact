using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageAsArchive : FileSystemBase
    {
        [Fact]
        public void GetBasic()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);
            using (Stream testContent = this.IndexReader.GetPackageAsArchive(testPackage.Name))
            {
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent);
                Assert.Single(items);
                Assert.Equal(testPackage.Content, items[testPackage.Path]);
            }
        }

        /// <summary>
        /// Ensures test coverage of code in GetPackageAsArchive() that references an existing archive
        /// file. To do this we hit the GetBasic() test, which generates an archive, then retrieve it again.
        /// The assert here is for completeness only - this test's passing requirement is measured in test
        /// coverage.
        /// 
        /// To confirm this test, uncomment it, run tests with coverage, generate report, and look for the branch
        /// warning in GetPackageAsArchive().
        /// </summary>
        [Fact]
        public void GetExistingArchive()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);
            using (Stream testContent1 = this.IndexReader.GetPackageAsArchive(testPackage.Name))
            {
                // get again
                using (Stream testContent2 = this.IndexReader.GetPackageAsArchive(testPackage.Name))
                {
                    Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent2);
                    Assert.Single(items);
                }
            }
        }

        /// <summary>
        /// Tests graceful handling of archive fetching for a non-existent package
        /// </summary>
        [Fact]
        public void GetNonExistent()
        {
            PackageNotFoundException ex = Assert.Throws<PackageNotFoundException>(() => {
                using (Stream zipStream = this.IndexReader.GetPackageAsArchive("invalid id"))
                {
                    // do nothing, exception expected
                }
            });

            Assert.Equal("invalid id", ex.PackageId);
        }

        /// <summary>
        /// Confirms that a timeout exception is thrown if package creation takes too long.
        /// </summary>
        [Fact]
        public void GetTimesOut()
        {
            // zero wait times to speed up test, this should trigger an instant timeout
            base.Settings.ArchiveWaitTimeout = 0;
            base.Settings.ArchiveAvailablePollInterval = 0;

            // we need a valid package first
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // mock a temp archive file and lock it to simulate an ongoing zip
            string tempArchivePath = this.IndexReader.GetPackageArchiveTempPath(testPackage.Name);
            File.WriteAllText(tempArchivePath, string.Empty);

            using (Stream lockStream = new FileStream(tempArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Assert.Throws<TimeoutException>(() => {
                    using (Stream zipStream = this.IndexReader.GetPackageAsArchive(testPackage.Name))
                    {
                        // do nothing, exception expected
                    }
                });
            }
        }

        /// <summary>
        /// Confirms that waiting-for-archive to generate goes into loop, which exits when the archive temp file is replaced with the completed archive.
        /// </summary>
        [Fact]
        public void GetAfterWaiting()
        {
            // we need a valid package first
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // create a fake archive temp file so GetPackageAsArchive() goes into wait state
            string tempArchivePath = this.IndexReader.GetPackageArchiveTempPath(testPackage.Name);
            File.WriteAllText(tempArchivePath, string.Empty);

            // seeing as our fake temp archive bypasses archive creation, we should make a fake archive too, it should just be a file
            string archivePath = this.IndexReader.GetPackageArchivePath(testPackage.Name);
            File.WriteAllText(archivePath, "some-fake-archive-content");

            // lock the temp archive so it doesn't get auto-clobbered
            using (Stream lockStream = new FileStream(tempArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // mock IThread.Sleep, in this method, we will release and delete our temp archive lock, so the archive fetch can proceed
                IThread mockThread = Mock.Of<IThread>();
                Mock.Get(mockThread)
                    .Setup(r => r.Sleep( It.IsAny<int>() ))
                    .Callback<int>(time =>{
                        lockStream.Close();
                        File.Delete(tempArchivePath);
                    });

                // make a custom reader with our mocked Thread
                IIndexReader indexReader = new Core.IndexReader(this.Settings, mockThread, this.TagService, this.Logger, new FileSystem(), HashServiceHelper.Instance());

                using (Stream zipStream = indexReader.GetPackageAsArchive(testPackage.Name))
                {
                    // confirm we got our fake archive content back
                    Assert.Equal("some-fake-archive-content", StreamsHelper.StreamToString(zipStream));
                }
            }
        }
    }
}
