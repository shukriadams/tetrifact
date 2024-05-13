using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class AddTag : Base 
    {
        [Fact]
        public void Happy_path(){
            TestPackage package = PackageHelper.CreateRandomPackage(SettingsHelper.CurrentSettingsContext);
            string[] tags = new[] { "mytag", "mytag3" };

            foreach (string tag in tags){
                base.TagsService.AddTag(package.Id, tag);
            }

            IEnumerable<Package> packages = base.PackageList.GetWithTags(tags, 0, 10);

            Assert.Single(packages);
            Assert.True(tags.IsSubsetOf(packages.ElementAt(0).Tags));
        }


        [Fact]
        public void InvalidPackage()
        {
            Assert.Throws<PackageNotFoundException>(()=>{ base.TagsService.AddTag("invalid-package-id", "some tag"); });
        }
    }
}
