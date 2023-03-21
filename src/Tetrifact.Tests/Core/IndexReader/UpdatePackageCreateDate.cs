using System;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class UpdatePackageCreateDate : FileSystemBase
    {
        /// <summary>
        /// Happy path - confirms that package verification works
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            TestPackage package = PackageHelper.CreateNewPackageFiles(Settings, "mypackage");
            Core.Manifest manifest = this.IndexReader.GetManifest(package.Id);
            Assert.Equal(DateTime.UtcNow.Year, manifest.CreatedUtc.Year);

            this.IndexReader.UpdatePackageCreateDate("mypackage", "2001-01-01");
            manifest = this.IndexReader.GetManifest(package.Id);
            Assert.Equal(2001, manifest.CreatedUtc.Year);
        }
    }
}