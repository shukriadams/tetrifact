using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests.TagsServoce
{
    public class GetAllTags : Base 
    {
        [Fact]
        public void Basic(){
            TestPackage package1 = this.CreatePackage("package1");
            string tag1 = "mytag1";
            base.TagsService.AddTag(package1.Name, tag1);

            TestPackage package2 = this.CreatePackage("package2");
            string tag2 = "mytag2";
            base.TagsService.AddTag(package2.Name, tag2);

            IEnumerable<string> tags = base.TagsService.GetAllTags();

            Assert.Equal(2, tags.Count());
            Assert.Contains(tag1, tags);
            Assert.Contains(tag2, tags);
        }
    }
}
