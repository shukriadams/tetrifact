using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.ArchiveService
{
    public class GetPackageArchiveStatus : FileSystemBase
    {
        /// <summary>
        /// Requesting an invalid package should throw proper exception
        /// </summary>
        [Fact]
        public void GetNonExistent()
        {
            Assert.Throws<PackageNotFoundException>(() => this.ArchiveService.GetPackageArchiveStatus("invalid-id"));
        }

        /// <summary>
        /// Archive create status should be 0 (started)
        /// </summary>
        [Fact]
        public void GetArchiveStarted()
        {
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);
            Assert.Equal(0, this.ArchiveService.GetPackageArchiveStatus(testPackage.Id));
        }

        /// <summary>
        /// Archive create status should be "already in progress".
        /// </summary>
        [Fact]
        public void GetArchiveInProgress()
        {
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);
            
            // mock temp archive (temp = in progress)
            File.WriteAllText(this.ArchiveService.GetPackageArchiveTempPath(testPackage.Id), string.Empty);

            Assert.Equal(1, this.ArchiveService.GetPackageArchiveStatus(testPackage.Id));
        }

        /// <summary>
        /// Archive status should be complete.
        /// </summary>
        [Fact]
        public void GetArchiveComplete()
        {
            TestPackage testPackage = PackageHelper.CreateNewPackage(this.Settings);

            // mock existing archive file
            File.WriteAllText(this.ArchiveService.GetPackageArchivePath(testPackage.Id), string.Empty);

            Assert.Equal(2, this.ArchiveService.GetPackageArchiveStatus(testPackage.Id));
        }
    }
}
