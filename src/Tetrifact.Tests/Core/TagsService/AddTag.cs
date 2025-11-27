using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class AddTag 
    {
        private TestContext _testContext = new TestContext();

        private PackageHelper _packageHelper;

        public AddTag()
        {
            _packageHelper = new PackageHelper(_testContext);
        }

        [Fact]
        public void Happy_path(){

            ITagsService tagService = _testContext.Get<ITagsService>();
            TestPackage package = _packageHelper.CreateRandomPackage();

            string[] tags = new[] { "mytag", "mytag3" };

            foreach (string tag in tags){
                tagService.AddTag(package.Id, tag);
            }

            IPackageListService packageList = _testContext.Get<IPackageListService>();
            IEnumerable<Package> packages = packageList.GetWithTags(tags, 0, 10);

            Assert.Single(packages);
            Assert.True(tags.IsSubsetOf(packages.ElementAt(0).Tags));
        }


        [Fact]
        public void InvalidPackage()
        {
            ITagsService tagsService = _testContext.Get<ITagsService>();
            Assert.Throws<PackageNotFoundException>(()=>{ tagsService.AddTag("invalid-package-id", "some tag"); });
        }
    }
}
