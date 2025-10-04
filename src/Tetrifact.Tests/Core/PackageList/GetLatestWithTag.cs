using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetLatestWithTag : Base
    {
        [Fact]
        public void BasicList()
        {
            ISettings settings = TestContext.Get<ISettings>();

            // list works by reading manifest json files on system. Create two manifests. All we need are dates on them.
            PackageHelper.WriteManifest(settings, new Manifest() { Id = "package2001", CreatedUtc = DateTime.Parse("2001/1/1") });
            PackageHelper.WriteManifest(settings, new Manifest() { Id = "package2002", CreatedUtc = DateTime.Parse("2002/1/1") });
            TagHelper.TagPackage(settings, "tag", "package2001");
            TagHelper.TagPackage(settings, "tag", "package2002");

            Package package = this.PackageList.GetLatestWithTags(new[]{"tag"});
            Assert.Equal("package2002", package.Id);
        }
    }
}
