using System.Collections.Generic;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetWithTag
    {
        private TestContext _testContext = new TestContext();
        
        private PackageHelper _packageHelper;

        public GetWithTag()
        {
            _packageHelper = new PackageHelper(_testContext);
        }
        
        [Fact]
        public void GetsBySingleTag()
        {
            ISettings settings = _testContext.Get<ISettings>();
            IPackageListService packageList = _testContext.Get<IPackageListService>();

            // tag list work by reading manifest json files on system. Create three manifests,  tag first two with one tag, and last with other tag
            _packageHelper.WriteManifest(new Manifest { Id = "package2003" });
            TagHelper.TagPackage(settings, "tag2", "package2003");
            TagHelper.TagPackage(settings, "tag4", "package2003");

            _packageHelper.WriteManifest(new Manifest { Id = "package2002" });
            TagHelper.TagPackage(settings, "tag2", "package2002");
            TagHelper.TagPackage(settings, "tag3", "package2002");

            _packageHelper.WriteManifest(new Manifest { Id = "package2001" });
            TagHelper.TagPackage(settings, "tag1", "package2001");
            TagHelper.TagPackage(settings, "tag5", "package2001");

            IEnumerable<Package> tags = packageList.GetWithTags(new[] { "tag2" }, 0, 2);
            Assert.Equal(2, tags.Count());
            Assert.Contains("tag2", tags.ElementAt(0).Tags);
        }

        [Fact]
        public void GetsByMultipleTags()
        {
            ISettings settings = _testContext.Get<ISettings>();
            IPackageListService packageList = _testContext.Get<IPackageListService>();

            // tag list work by reading manifest json files on system. Create three manifests,  tag first two with one tag, and last with other tag
            _packageHelper.WriteManifest(new Manifest { Id = "package2003" });
            TagHelper.TagPackage(settings, "tag2", "package2003");
            TagHelper.TagPackage(settings, "tag4", "package2003");

            _packageHelper.WriteManifest(new Manifest { Id = "package2002" });
            TagHelper.TagPackage(settings, "tag2", "package2002");
            TagHelper.TagPackage(settings, "tag3", "package2002");

            _packageHelper.WriteManifest(new Manifest { Id = "package2001" });
            TagHelper.TagPackage(settings, "tag1", "package2001");
            TagHelper.TagPackage(settings, "tag5", "package2001");


            IEnumerable<Package> tags = packageList.GetWithTags(new[] { "tag2", "tag3" }, 0, 2);
            Assert.Single(tags);
            Assert.Contains("tag2", tags.ElementAt(0).Tags);
            Assert.Contains("tag3", tags.ElementAt(0).Tags);
        }
    }
}
