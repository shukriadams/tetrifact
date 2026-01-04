using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;

namespace Tetrifact.Tests.TagsService
{
    public class PackageDelete 
    {
        private TestContext _testContext = new TestContext();

        private PackageHelper _packageHelper;

        public PackageDelete()
        {
            _packageHelper = new PackageHelper(_testContext);
        }

        /// <summary>
        /// Confirms that deleting a package removes its tag reference.
        ///
        /// This test actually belongs to IndexReader, but it combines tagging logic, and placing it here generated least code clutter.
        /// </summary>
        [Fact]
        public void Basic() {
            TestPackage package = _packageHelper.CreateRandomPackage();
            ITagsService tagsService = _testContext.Instantiate<ITagsService>();
            IIndexReadService indexReader = _testContext.Instantiate<IIndexReadService>();

            string[] tags = new[] { "mytag" };

            foreach (string tag in tags) { 
               tagsService.AddTag(package.Id, tag);
            }

            indexReader.DeletePackage(package.Id);
            IPackageListService packageList = _testContext.Instantiate<IPackageListService>();

            IEnumerable<Package> packages = packageList.GetWithTags(tags, 0, 10);
            Assert.Empty(packages);
        }
    }
}
