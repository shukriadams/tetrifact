using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetWithTag : TestBase
    {
        [Fact]
        public void GetsBySingleTag()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IPackageListService packageList = TestContext.Get<IPackageListService>();

            // tag list work by reading manifest json files on system. Create three manifests,  tag first two with one tag, and last with other tag
            PackageHelper.WriteManifest(new Manifest { Id = "package2003" });
            TagHelper.TagPackage(settings, "tag2", "package2003");
            TagHelper.TagPackage(settings, "tag4", "package2003");

            PackageHelper.WriteManifest(new Manifest { Id = "package2002" });
            TagHelper.TagPackage(settings, "tag2", "package2002");
            TagHelper.TagPackage(settings, "tag3", "package2002");

            PackageHelper.WriteManifest(new Manifest { Id = "package2001" });
            TagHelper.TagPackage(settings, "tag1", "package2001");
            TagHelper.TagPackage(settings, "tag5", "package2001");

            IEnumerable<Package> tags = packageList.GetWithTags(new[] { "tag2" }, 0, 2);
            Assert.Equal(2, tags.Count());
            Assert.Contains("tag2", tags.ElementAt(0).Tags);
        }

        [Fact]
        public void GetsByMultipleTags()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IPackageListService packageList = TestContext.Get<IPackageListService>();

            // tag list work by reading manifest json files on system. Create three manifests,  tag first two with one tag, and last with other tag
            PackageHelper.WriteManifest(new Manifest { Id = "package2003" });
            TagHelper.TagPackage(settings, "tag2", "package2003");
            TagHelper.TagPackage(settings, "tag4", "package2003");

            PackageHelper.WriteManifest(new Manifest { Id = "package2002" });
            TagHelper.TagPackage(settings, "tag2", "package2002");
            TagHelper.TagPackage(settings, "tag3", "package2002");

            PackageHelper.WriteManifest(new Manifest { Id = "package2001" });
            TagHelper.TagPackage(settings, "tag1", "package2001");
            TagHelper.TagPackage(settings, "tag5", "package2001");


            IEnumerable<Package> tags = packageList.GetWithTags(new[] { "tag2", "tag3" }, 0, 2);
            Assert.Single(tags);
            Assert.Contains("tag2", tags.ElementAt(0).Tags);
            Assert.Contains("tag3", tags.ElementAt(0).Tags);
        }
    }
}
