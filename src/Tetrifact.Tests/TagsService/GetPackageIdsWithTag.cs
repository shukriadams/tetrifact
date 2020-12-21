using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class GetPackageIdsWithTag : Base 
    {
        [Fact]
        public void Basic(){
            string[] tags = new [] { "mytag" };

            TestPackage package1 = this.CreatePackage("package1");
            TestPackage package2 = this.CreatePackage("package2");
            foreach (string tag in tags) {
                base.TagsService.AddTag(package1.Name, tag);
                base.TagsService.AddTag(package2.Name, tag);
            }

            IEnumerable<string> packageIds = base.TagsService.GetPackageIdsWithTags(tags);

            Assert.Equal(2, packageIds.Count());
            Assert.Contains("package1", packageIds);
            Assert.Contains("package2", packageIds);
        }
    }
}
