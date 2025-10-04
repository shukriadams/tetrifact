using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;

namespace Tetrifact.Tests.TagsService
{
    public class PackageDelete : Base 
    {
        /// <summary>
        /// Confirms that deleting a package removes its tag reference.
        ///
        /// This test actually belongs to IndexReader, but it combines tagging logic, and placing it here generated least code clutter.
        /// </summary>
        [Fact]
        public void Basic() {
            TestPackage package = PackageHelper.CreateRandomPackage();
            ITagsService tagsService = TestContext.Get<ITagsService>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            string[] tags = new[] { "mytag" };

            foreach (string tag in tags) { 
               tagsService.AddTag(package.Id, tag);
            }

            indexReader.DeletePackage(package.Id);
            IPackageListService packageList = TestContext.Get<IPackageListService>();

            IEnumerable<Package> packages = packageList.GetWithTags(tags, 0, 10);
            Assert.Empty(packages);
        }
    }
}
