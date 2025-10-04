using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
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
            ISettings settings = TestContext.Get<ISettings>();

            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package1"));
            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package2"));
            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package3"));

            IEnumerable<string> packages = this.IndexReader.GetAllPackageIds();
            Assert.Equal(3, packages.Count());
            Assert.Contains("package1", packages);
            Assert.Contains("package2", packages);
            Assert.Contains("package3", packages);
        }
    }
}
