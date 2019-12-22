using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class AddTag : Base 
    {
        /// <summary>
        /// Adds a tag to a package
        /// </summary>
        [Fact]
        public void Basic(){
            DummyPackage package = this.CreatePackage();
            string tag = "mytag";
            base.TagsService.AddTag("some-project", package.Id, tag);
            IEnumerable<Package> packages = base.PackageList.GetWithTag("some-project", tag, 0, 10);

            Assert.Single(packages);
            Assert.Contains(tag, packages.ElementAt(0).Tags);
        }
    }
}
