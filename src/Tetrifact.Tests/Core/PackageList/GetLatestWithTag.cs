using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetLatestWithTag
    {
        private TestContext _testContext = new TestContext();
        
        private PackageHelper _packageHelper;

        public GetLatestWithTag()
        {
            _packageHelper = new PackageHelper(_testContext);
        }

        [Fact]
        public void BasicList()
        {
            ISettings settings = _testContext.Get<ISettings>();
            IPackageListService packageList = _testContext.Get<IPackageListService>();

            // list works by reading manifest json files on system. Create two manifests. All we need are dates on them.
            _packageHelper.WriteManifest(new Manifest() { Id = "package2001", CreatedUtc = DateTime.Parse("2001/1/1") });
            _packageHelper.WriteManifest(new Manifest() { Id = "package2002", CreatedUtc = DateTime.Parse("2002/1/1") }); 
            TagHelper.TagPackage(settings, "tag", "package2001");
            TagHelper.TagPackage(settings, "tag", "package2002");

            Package package = packageList.GetLatestWithTags(new[]{"tag"});
            Assert.Equal("package2002", package.Id);
        }
    }
}
