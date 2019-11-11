using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;

namespace Tetrifact.Tests.TagsService
{
    public class RemoveTag : Base 
    {
        [Fact]
        public void Basic(){
            TestPackage package = this.CreatePackage();
            string tag = "mytag";
            base.TagsService.AddTag("some-project", package.Name, tag);
            base.TagsService.RemoveTag("some-project", package.Name, tag);

            IEnumerable<Package> packages = base.PackageList.GetWithTag("some-project", tag, 0, 10);
            Assert.Empty(packages);
        }
    }
}
