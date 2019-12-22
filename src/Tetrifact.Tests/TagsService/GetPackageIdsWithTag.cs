using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsService
{
    public class GetPackageIdsWithTag : Base 
    {
        [Fact]
        public void Basic(){
            string tag = "mytag";

            DummyPackage package1 = this.CreatePackage("package1");
            base.TagsService.AddTag("some-project", package1.Id, tag);

            DummyPackage package2 = this.CreatePackage("package2");
            base.TagsService.AddTag("some-project", package2.Id, tag);

            IEnumerable<string> packageIds = base.PackageList.GetPackagesWithTag("some-project", tag);

            Assert.Equal(2, packageIds.Count());
            Assert.Contains("package1", packageIds);
            Assert.Contains("package2", packageIds);
        }
    }
}
