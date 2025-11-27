using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetLatestWithTag : TestBase
    {
        [Fact]
        public void BasicList()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IPackageListService packageList = TestContext.Get<IPackageListService>();

            // list works by reading manifest json files on system. Create two manifests. All we need are dates on them.
            PackageHelper.WriteManifest(new Manifest() { Id = "package2001", CreatedUtc = DateTime.Parse("2001/1/1") });
            PackageHelper.WriteManifest(new Manifest() { Id = "package2002", CreatedUtc = DateTime.Parse("2002/1/1") }); 
            TagHelper.TagPackage(settings, "tag", "package2001");
            TagHelper.TagPackage(settings, "tag", "package2002");

            Package package = packageList.GetLatestWithTags(new[]{"tag"});
            Assert.Equal("package2002", package.Id);
        }
    }
}
