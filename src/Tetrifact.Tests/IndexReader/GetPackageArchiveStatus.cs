using System.IO;
using Tetrifact.Core;
using Xunit;
using Tetrifact.Dev;

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
            Assert.Throws<PackageNotFoundException>(() => this.IndexReader.GetPackageArchiveStatus("some-project", "invalid-id"));
        }

        [Fact]
        public void GetDefault()
        {
            DummyPackage testPackage = this.CreatePackage();
            Assert.Equal(0, this.IndexReader.GetPackageArchiveStatus("some-project", testPackage.Id));
        }

        [Fact]
        public void GetInProgressStatus()
        {
            DummyPackage testPackage = this.CreatePackage();
            
            // mock temp archive
            File.WriteAllText(this.IndexReader.GetPackageArchiveTempPath("some-project", testPackage.Id), string.Empty);

            Assert.Equal(1, this.IndexReader.GetPackageArchiveStatus("some-project", testPackage.Id));
        }

        [Fact]
        public void GetReadyStatus()
        {
            DummyPackage testPackage = this.CreatePackage();

            // mock temp archive

            File.WriteAllText(this.IndexReader.GetPackageArchivePath("some-project", testPackage.Id), string.Empty);

            Assert.Equal(2, this.IndexReader.GetPackageArchiveStatus("some-project", testPackage.Id));
        }
    }
}
