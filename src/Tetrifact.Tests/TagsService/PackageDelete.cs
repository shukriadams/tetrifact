using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsServoce
{
    public class PackageDelete : Base 
    {
        /// <summary>
        /// Confirms that deleting a package removes its tag reference.
        ///
        /// This test actually belongs to IndexReader, but it combines tagging logic, and placing it here generated least code clutter.
        /// </summary>
        [Fact]
        public void Basic(){
            TestPackage package = this.CreatePackage();
            string tag = "mytag";
            base.TagsService.AddTag(package.Name, tag);
            base.IndexReader.DeletePackage(package.Name);

            IEnumerable<Package> packages = base.PackageList.GetWithTag(tag, 0, 10);
            Assert.Empty(packages);
        }
    }
}
