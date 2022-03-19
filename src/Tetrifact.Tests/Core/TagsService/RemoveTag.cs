using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;

namespace Tetrifact.Tests.TagsService
{
    public class RemoveTag : Base 
    {
        [Fact]
        public void Basic(){

            TestPackage package = PackageHelper.CreateNewPackage(this.Settings);
            string[] tags = new[] { "mytag" };

            foreach (string tag in tags) {
                base.TagsService.AddTag(package.Id, tag);
                base.TagsService.RemoveTag(package.Id, tag);
            }

            IEnumerable<Package> packages = base.PackageList.GetWithTags(tags, 0, 10);
            Assert.Empty(packages);
        }

        [Fact]
        public void InvalidPackage()
        {
            Assert.Throws<PackageNotFoundException>(() => { base.TagsService.RemoveTag("invalid-package-id", "some tag"); });
        }

    }
}
