using Xunit;
using System.Collections.Generic;
using System.Linq;
using Tetrifact.Dev;

namespace Tetrifact.Tests.TagsService
{
    public class GetAllTags : Base 
    {
        [Fact]
        public void Basic(){
            DummyPackage package1 = this.CreatePackage("package1");
            string tag1 = "mytag1";
            base.TagsService.AddTag("some-project", package1.Id, tag1);

            DummyPackage package2 = this.CreatePackage("package2");
            string tag2 = "mytag2";
            base.TagsService.AddTag("some-project", package2.Id, tag2);

            IEnumerable<string> tags = base.PackageList.GetAllTags("some-project");

            Assert.Equal(2, tags.Count());
            Assert.Contains(tag1, tags);
            Assert.Contains(tag2, tags);
        }
    }
}
