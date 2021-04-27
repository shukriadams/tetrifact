using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class AddTag : Base 
    {
        [Fact]
        public void Basic(){
            TestPackage package = PackageHelper.CreatePackage(this.Settings);
            string[] tags = new[] { "mytag", "mytag3" };

            foreach (string tag in tags){
                base.TagsService.AddTag(package.Name, tag);
            }

            IEnumerable<Package> packages = base.PackageList.GetWithTags(tags, 0, 10);

            Assert.Single(packages);
            Assert.True(tags.IsSubsetOf(packages.ElementAt(0).Tags));
        }
    }
}
