using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class UpdatePackageCreateDate : TestBase
    {
        /// <summary>
        /// Happy path - confirms that package verification works
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            TestPackage package = PackageHelper.CreateNewPackageFiles("mypackage");
            Manifest manifest = indexReader.GetManifest(package.Id);
            Assert.Equal(DateTime.UtcNow.Year, manifest.CreatedUtc.Year);

            indexReader.UpdatePackageCreateDate("mypackage", "2001-01-01");
            manifest = indexReader.GetManifest(package.Id);
            Assert.Equal(2001, manifest.CreatedUtc.Year);
        }
    }
}