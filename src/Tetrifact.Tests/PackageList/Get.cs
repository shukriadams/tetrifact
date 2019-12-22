using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class Get : Base
    {
        /// <summary>
        /// PackageList returns a list of all existing packages
        /// </summary>
        [Fact]
        public void Basic()
        {
            this.CreatePackage("package2003");
            this.CreatePackage("package2002");
            this.CreatePackage("package2001");

            IList<Package> packages = this.PackageList.Get("some-project", 0, 10).ToList();

            Assert.Equal(3, packages.Count());
            Assert.NotEmpty(packages.Where(r => r.Id == "package2001"));
            Assert.NotEmpty(packages.Where(r => r.Id == "package2002"));
            Assert.NotEmpty(packages.Where(r => r.Id == "package2003"));
        }

        /// <summary>
        /// Confirms graceful handling of packages with corrupt json
        /// </summary>
        [Fact]
        public void GracefullyHandleInvalidJSON()
        {
            // create two packages, get one's manifest path. Overwrite that manifest with invalid json
            string packageName1 = "package2003";
            string packageName2 = "package2004";
            this.CreatePackage(packageName1);
            this.CreatePackage(packageName2);
            Manifest manifest = IndexReader.GetManifest("some-project", packageName1);
            File.WriteAllText(manifest.PathOnDisk, "definitely not some valid json");

            IList<Package> packages = this.PackageList.Get("some-project", 0, 10).ToList();

            Assert.Single(packages);
            Assert.NotEmpty(packages.Where(r => r.Id == packageName2));
        }
    }
}
