using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageArchiveStatus : FileSystemBase
    {
        /// <summary>
        /// Tests graceful handling of requesting an invalid package
        /// </summary>
        [Fact]
        public void GetNonExistent()
        {
            this.InitProject();
            Assert.Throws<PackageNotFoundException>(() => this.IndexReader.GetPackageArchiveStatus("some-project", "invalid-id"));
        }

        [Fact]
        public void GetDefault()
        {
            TestPackage testPackage = this.CreatePackage();
            Assert.Equal(0, this.IndexReader.GetPackageArchiveStatus("some-project", testPackage.Name));
        }

        [Fact]
        public void GetInProgressStatus()
        {
            TestPackage testPackage = this.CreatePackage();
            
            // mock temp archive
            File.WriteAllText(this.IndexReader.GetPackageArchiveTempPath("some-project", testPackage.Name), string.Empty);

            Assert.Equal(1, this.IndexReader.GetPackageArchiveStatus("some-project", testPackage.Name));
        }

        [Fact]
        public void GetReadyStatus()
        {
            TestPackage testPackage = this.CreatePackage();

            // mock temp archive

            File.WriteAllText(this.IndexReader.GetPackageArchivePath("some-project", testPackage.Name), string.Empty);

            Assert.Equal(2, this.IndexReader.GetPackageArchiveStatus("some-project", testPackage.Name));
        }
    }
}
