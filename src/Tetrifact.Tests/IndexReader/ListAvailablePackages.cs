using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class ListAvailablePackages : Base
    {
        [Fact]
        public void ListAll()
        {
            Directory.CreateDirectory(Path.Join(this.Settings.PackagePath, "package1"));
            Directory.CreateDirectory(Path.Join(this.Settings.PackagePath, "package2"));
            Directory.CreateDirectory(Path.Join(this.Settings.PackagePath, "package3"));

            IEnumerable<string> packages = this.IndexReader.GetPackageIds(0, 10);
            Assert.Equal(3, packages.Count());
            Assert.Contains("package1", packages);
            Assert.Contains("package2", packages);
            Assert.Contains("package3", packages);
        }
    }
}
