using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;

namespace Tetrifact.Tests.TagsService
{
    public class RemoveTag : Base 
    {
        [Fact]
        public void Basic(){

            TestPackage package = PackageHelper.CreatePackage(this.Settings);
            string[] tags = new[] { "mytag" };

            foreach (string tag in tags) {
                base.TagsService.AddTag(package.Name, tag);
                base.TagsService.RemoveTag(package.Name, tag);
            }

            IEnumerable<Package> packages = base.PackageList.GetWithTags(tags, 0, 10);
            Assert.Empty(packages);
        }
    }
}
