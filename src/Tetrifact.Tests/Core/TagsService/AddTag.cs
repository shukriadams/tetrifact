using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class AddTag : TestBase 
    {
        [Fact]
        public void Happy_path(){

            ITagsService tagService = TestContext.Get<ITagsService>();
            TestPackage package = PackageHelper.CreateRandomPackage();

            string[] tags = new[] { "mytag", "mytag3" };

            foreach (string tag in tags){
                tagService.AddTag(package.Id, tag);
            }

            IPackageListService packageList = TestContext.Get<IPackageListService>();
            IEnumerable<Package> packages = packageList.GetWithTags(tags, 0, 10);

            Assert.Single(packages);
            Assert.True(tags.IsSubsetOf(packages.ElementAt(0).Tags));
        }


        [Fact]
        public void InvalidPackage()
        {
            ITagsService tagsService = TestContext.Get<ITagsService>();
            Assert.Throws<PackageNotFoundException>(()=>{ tagsService.AddTag("invalid-package-id", "some tag"); });
        }
    }
}
