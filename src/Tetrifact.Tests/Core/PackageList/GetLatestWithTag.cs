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
            ISettings settings = _testContext.Instantiate<ISettings>();
            IPackageListService packageList = _testContext.Instantiate<IPackageListService>();

            // GetLatestWithTags works by reading manifest json files and getting dates from them. 
            // All we need are two manifests with hardcoded dates in them.
            _packageHelper.WriteManifest(new Manifest() { Id = "package2001", CreatedUtc = DateTime.Parse("2001/1/1") });
            _packageHelper.WriteManifest(new Manifest() { Id = "package2002", CreatedUtc = DateTime.Parse("2002/1/1") }); 

            // tag the two packages by static id's 
            TagHelper.TagPackage(settings, "mytag", "package2001");
            TagHelper.TagPackage(settings, "mytag", "package2002");

            // retrieve package by tag
            Package package = packageList.GetLatestWithTags(new[]{"mytag"});

            Assert.Equal("package2002", package.Id);
        }
    }
}
