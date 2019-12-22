using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetWithTag : Base
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

            IEnumerable<Package> tags = this.PackageList.GetWithTag("some-project", "tag2", 0, 2);
            Assert.Equal(2, tags.Count());
            Assert.Contains("tag2", tags.ElementAt(0).Tags);
        }
    }
}
