using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class GetPackageIdsWithTag : Base 
    {
        [Fact]
        public void Basic(){
            string tag = "mytag";

            TestPackage package1 = this.CreatePackage("package1");
            base.TagsService.AddTag("some-project", package1.Name, tag);

            TestPackage package2 = this.CreatePackage("package2");
            base.TagsService.AddTag("some-project", package2.Name, tag);

            IEnumerable<string> packageIds = base.TagsService.GetPackagesWithTag("some-project", tag);

            Assert.Equal(2, packageIds.Count());
            Assert.Contains("package1", packageIds);
            Assert.Contains("package2", packageIds);
        }
    }
}
