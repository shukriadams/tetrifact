using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class AddTag : Base 
    {
        [Fact]
        public void Basic(){
            TestPackage package = this.CreatePackage();
            string tag = "mytag";
            base.TagsService.AddTag("some-project", package.Name, tag);
            IEnumerable<Package> packages = base.PackageList.GetWithTag("some-project", tag, 0, 10);

            Assert.Single(packages);
            Assert.Contains(tag, packages.ElementAt(0).Tags);
        }
    }
}
