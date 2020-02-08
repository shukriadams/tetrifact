using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    [Collection("Tests")]
    public class GetPopularTags : Base
    {
        [Fact]
        public void Basic()
        {
            this.CreatePackage("package2001");
            this.CreatePackage("package2002");
            this.CreatePackage("package2003");

            this.TagService.AddTag("some-project", "package2001", "tag1");
            this.TagService.AddTag("some-project", "package2002", "tag2");
            this.TagService.AddTag("some-project", "package2003", "tag2");


            IEnumerable<string> tags = this.PackageList.GetPopularTags("some-project", 3);
            Assert.Equal("tag2", tags.First());
            Assert.Equal("tag1", tags.ElementAt(1));
        }
    }
}
