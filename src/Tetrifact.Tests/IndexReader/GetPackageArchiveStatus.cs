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
            Assert.Throws<PackageNotFoundException>(() => this.IndexReader.GetPackageArchiveStatus("invalid-id"));
        }

        [Fact]
        public void GetDefault()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);
            Assert.Equal(0, this.IndexReader.GetPackageArchiveStatus(testPackage.Id));
        }

        [Fact]
        public void GetInProgressStatus()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);
            
            // mock temp archive
            File.WriteAllText(this.IndexReader.GetPackageArchiveTempPath(testPackage.Id), string.Empty);

            Assert.Equal(1, this.IndexReader.GetPackageArchiveStatus(testPackage.Id));
        }

        [Fact]
        public void GetReadyStatus()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // mock temp archive

            File.WriteAllText(this.IndexReader.GetPackageArchivePath(testPackage.Id), string.Empty);

            Assert.Equal(2, this.IndexReader.GetPackageArchiveStatus(testPackage.Id));
        }
    }
}
