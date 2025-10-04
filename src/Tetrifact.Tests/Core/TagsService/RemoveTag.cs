using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;

namespace Tetrifact.Tests.TagsService
{
    public class RemoveTag : Base 
    {
        [Fact]
        public void Basic(){

            TestPackage package = PackageHelper.CreateRandomPackage();
            ITagsService tagsService = TestContext.Get<ITagsService>();

            string[] tags = new[] { "mytag" };

            foreach (string tag in tags) {
                tagsService.AddTag(package.Id, tag);
                tagsService.RemoveTag(package.Id, tag);
            }

            IPackageListService packageList = TestContext.Get<IPackageListService>();
            IEnumerable<Package> packages = packageList.GetWithTags(tags, 0, 10);
            Assert.Empty(packages);
        }

        [Fact]
        public void InvalidPackage()
        {
            ITagsService tagsService = TestContext.Get<ITagsService>();
            Assert.Throws<PackageNotFoundException>(() => { tagsService.RemoveTag("invalid-package-id", "some tag"); });
        }

    }
}
