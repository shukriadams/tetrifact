using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;

namespace Tetrifact.Tests.TagsService
{
    public class RemoveTag 
    {
        private TestContext _testContext = new TestContext();

        private PackageHelper _packageHelper;

        public RemoveTag()
        {
            _packageHelper = new PackageHelper(_testContext);
        }
        
        [Fact]
        public void Basic(){

            TestPackage package = _packageHelper.CreateRandomPackage();
            ITagsService tagsService = _testContext.Get<ITagsService>();

            string[] tags = new[] { "mytag" };

            foreach (string tag in tags) {
                tagsService.AddTag(package.Id, tag);
                tagsService.RemoveTag(package.Id, tag);
            }

            IPackageListService packageList = _testContext.Get<IPackageListService>();
            IEnumerable<Package> packages = packageList.GetWithTags(tags, 0, 10);
            Assert.Empty(packages);
        }

        [Fact]
        public void InvalidPackage()
        {
            ITagsService tagsService = _testContext.Get<ITagsService>();
            Assert.Throws<PackageNotFoundException>(() => { tagsService.RemoveTag("invalid-package-id", "some tag"); });
        }

    }
}
