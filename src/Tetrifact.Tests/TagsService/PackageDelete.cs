using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

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
            TestPackage package = this.CreatePackage();
            string[] tags = new[] { "mytag" };

            foreach (string tag in tags) { 
               base.TagsService.AddTag(package.Name, tag);
            }

            base.IndexReader.DeletePackage(package.Name);
            IEnumerable<Package> packages = base.PackageList.GetWithTags(tags, 0, 10);
            Assert.Empty(packages);
        }
    }
}
