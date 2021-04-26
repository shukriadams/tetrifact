using System.Collections.Generic;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetPopularTags : Base
    {
        /// <summary>
        /// A single tag should not show up on popular list
        /// </summary>
        [Fact]
        public void NoSingles()
        {
            PackageHelper.WritePackage(this.Settings, new Manifest() { Id = "package" });
            TagHelper.TagPackage(this.Settings, "tag", "package");

            IEnumerable<string> tags = this.PackageList.GetPopularTags(3);
            Assert.Empty(tags);

        }

        [Fact]
        public void Basic()
        {
            // tag list work by reading manifest json files on system. Create three manifests,  tag first two with one tag, and last with other tag
            PackageHelper.WritePackage(this.Settings, new Manifest() { Id = "package2004" });
            PackageHelper.WritePackage(this.Settings, new Manifest() { Id = "package2003" });
            PackageHelper.WritePackage(this.Settings, new Manifest() { Id = "package2002" });
            
            TagHelper.TagPackage(this.Settings, "tag2", "package2004");
            TagHelper.TagPackage(this.Settings, "tag2", "package2003");
            TagHelper.TagPackage(this.Settings, "tag2", "package2002");
            TagHelper.TagPackage(this.Settings, "tag1", "package2004");
            TagHelper.TagPackage(this.Settings, "tag1", "package2003");

            IEnumerable<string> tags = this.PackageList.GetPopularTags(3);
            Assert.Equal("tag2", tags.First());
            Assert.Equal("tag1", tags.ElementAt(1));

        }
    }
}
