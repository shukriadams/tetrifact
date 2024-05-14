using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetAllPackageIds : FileSystemBase
    {
        /// <summary>
        /// Confirms that methods returns a list of directory basenames from within packages directory
        /// </summary>
        [Fact]
        public void GetBasic()
        {
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package1"));
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package2"));
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package3"));

            IEnumerable<string> packages = this.IndexReader.GetAllPackageIds();
            Assert.Equal(3, packages.Count());
            Assert.Contains("package1", packages);
            Assert.Contains("package2", packages);
            Assert.Contains("package3", packages);
        }
    }
}
