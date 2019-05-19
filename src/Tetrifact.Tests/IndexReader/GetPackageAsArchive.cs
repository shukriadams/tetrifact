using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageAsArchive : FileSystemBase
    {
        [Fact]
        public void GetBasic()
        {
            TestPackage testPackage = this.CreatePackage();
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
            TestPackage testPackage = this.CreatePackage();
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
            // zero wait times to speed up test
            base.Settings.ArchiveWaitTimeout = 0;
            base.Settings.ArchiveAvailablePollInterval = 0;

            // we need a valid package first
            TestPackage testPackage = this.CreatePackage();

            // mock a temp archive file, this means actual archive creation will be skipped and
            // therefore never complete.
            string tempArchivePath = this.IndexReader.GetPackageArchiveTempPath(testPackage.Name);
            File.WriteAllText(tempArchivePath, string.Empty);

            Assert.Throws<TimeoutException>(() =>{
                using (Stream zipStream = this.IndexReader.GetPackageAsArchive(testPackage.Name))
                {
                    // do nothing, exception expected
                }
            });
        }
        
        private class TestPackage
        {
            public byte[] Content;
            public string Path;
            public string Name;
        }

        /// <summary>
        /// Generates a valid package, returns its unique id.
        /// </summary>
        /// <returns></returns>
        private TestPackage CreatePackage()
        {
            TestPackage testPackage = new TestPackage();

            // create package, files folder and item location in one
            testPackage.Content = Encoding.ASCII.GetBytes("some content");
            testPackage.Path = "path/to/file";
            testPackage.Name = "somepackage";

            IWorkspace workspace = new Core.Workspace(this.Settings);
            workspace.AddIncomingFile(StreamsHelper.StreamFromString("some content"), testPackage.Path);
            workspace.WriteFile(testPackage.Path, "somehash", testPackage.Name);
            workspace.WriteManifest(testPackage.Name, "somehash2");

            return testPackage;
        }
    }
}
