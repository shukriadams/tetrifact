using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tetrifact.Core;
using Xunit;
using Tetrifact.Dev;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageAsArchive : FileSystemBase
    {
        [Fact]
        public void GetBasic()
        {
            DummyPackage testPackage = base.CreatePackage();
            using (Stream testContent = this.IndexReader.GetPackageAsArchive("some-project", testPackage.Id))
            {
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent);
                Assert.Single(items);
                Assert.Equal(Encoding.ASCII.GetBytes(testPackage.Files[0].Content),  items[testPackage.Files[0].Path]);
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
            DummyPackage testPackage = base.CreatePackage();
            using (Stream testContent1 = this.IndexReader.GetPackageAsArchive("some-project", testPackage.Id))
            {
                // get again
                using (Stream testContent2 = this.IndexReader.GetPackageAsArchive("some-project", testPackage.Id))
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
                using (Stream zipStream = this.IndexReader.GetPackageAsArchive("some-project", "invalid id"))
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
            // zero wait times to speed up test
            base.Settings.ArchiveWaitTimeout = 0;
            base.Settings.ArchiveAvailablePollInterval = 0;

            // we need a valid package first
            DummyPackage testPackage = base.CreatePackage();

            // mock a temp archive file, this means actual archive creation will be skipped and
            // therefore never complete.
            string tempArchivePath = this.IndexReader.GetPackageArchiveTempPath("some-project", testPackage.Id);
            File.WriteAllText(tempArchivePath, string.Empty);

            Assert.Throws<TimeoutException>(() =>{
                using (Stream zipStream = this.IndexReader.GetPackageAsArchive("some-project", testPackage.Id))
                {
                    // do nothing, exception expected
                }
            });
        }
    }
}
