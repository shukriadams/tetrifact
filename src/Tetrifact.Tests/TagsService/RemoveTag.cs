using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class RemoveTag : Base 
    {
        [Fact]
        public void Basic(){
            TestPackage package = this.CreatePackage();
            string tag = "mytag";
            base.TagsService.AddTag(package.Name, tag);
            base.TagsService.RemoveTag(package.Name, tag);

            IEnumerable<Package> packages = base.PackageList.GetWithTag(tag, 0, 10);
            Assert.Empty(packages);
        }
    }
}
