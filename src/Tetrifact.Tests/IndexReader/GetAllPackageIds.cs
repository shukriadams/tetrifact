using System.Collections.Generic;
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
            this.CreatePackage("package1");
            this.CreatePackage("package2");
            this.CreatePackage("package3");

            IEnumerable<string> packages = this.IndexReader.GetAllPackageIds("some-project");
            Assert.Equal(3, packages.Count());
            Assert.Contains("package1", packages);
            Assert.Contains("package2", packages);
            Assert.Contains("package3", packages);
        }
    }
}
