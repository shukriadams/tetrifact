using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetWithTag : Base
    {
        [Fact]
        public void GetsBySingleTag()
        {
            // tag list work by reading manifest json files on system. Create three manifests,  tag first two with one tag, and last with other tag
            PackageHelper.WriteManifest(SettingsHelper.CurrentSettingsContext, new Manifest { Id = "package2003" });
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag2", "package2003");
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag4", "package2003");

            PackageHelper.WriteManifest(SettingsHelper.CurrentSettingsContext, new Manifest { Id = "package2002" });
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag2", "package2002");
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag3", "package2002");

            PackageHelper.WriteManifest(SettingsHelper.CurrentSettingsContext, new Manifest { Id = "package2001" });
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag1", "package2001");
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag5", "package2001");

            IEnumerable<Package> tags = this.PackageList.GetWithTags(new[] { "tag2" }, 0, 2);
            Assert.Equal(2, tags.Count());
            Assert.Contains("tag2", tags.ElementAt(0).Tags);
        }

        [Fact]
        public void GetsByMultipleTags()
        {
            // tag list work by reading manifest json files on system. Create three manifests,  tag first two with one tag, and last with other tag
            PackageHelper.WriteManifest(SettingsHelper.CurrentSettingsContext, new Manifest { Id = "package2003" });
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag2", "package2003");
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag4", "package2003");

            PackageHelper.WriteManifest(SettingsHelper.CurrentSettingsContext, new Manifest { Id = "package2002" });
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag2", "package2002");
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag3", "package2002");

            PackageHelper.WriteManifest(SettingsHelper.CurrentSettingsContext, new Manifest { Id = "package2001" });
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag1", "package2001");
            TagHelper.TagPackage(SettingsHelper.CurrentSettingsContext, "tag5", "package2001");


            IEnumerable<Package> tags = this.PackageList.GetWithTags(new[] { "tag2", "tag3" }, 0, 2);
            Assert.Single(tags);
            Assert.Contains("tag2", tags.ElementAt(0).Tags);
            Assert.Contains("tag3", tags.ElementAt(0).Tags);
        }
    }
}
