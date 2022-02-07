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
            Assert.Throws<PackageNotFoundException>(() => this.ArchiveService.GetPackageArchiveStatus("invalid-id"));
        }

        [Fact]
        public void GetDefault()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);
            Assert.Equal(0, this.ArchiveService.GetPackageArchiveStatus(testPackage.Id));
        }

        [Fact]
        public void GetInProgressStatus()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);
            
            // mock temp archive
            File.WriteAllText(this.ArchiveService.GetPackageArchiveTempPath(testPackage.Id), string.Empty);

            Assert.Equal(1, this.ArchiveService.GetPackageArchiveStatus(testPackage.Id));
        }

        [Fact]
        public void GetReadyStatus()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // mock temp archive

            File.WriteAllText(this.ArchiveService.GetPackageArchivePath(testPackage.Id), string.Empty);

            Assert.Equal(2, this.ArchiveService.GetPackageArchiveStatus(testPackage.Id));
        }
    }
}
