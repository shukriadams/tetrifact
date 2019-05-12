using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class HandleCorruptPackage : Base
    {
        [Fact]
        public void GracefullyHandleInvalidJSON()
        {
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package_one"));
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "invalidPackage"));

            File.WriteAllText(Path.Combine(Settings.PackagePath, "package_one", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            // write a manifest file that consists of invalid JSON
            File.WriteAllText(Path.Combine(Settings.PackagePath, "invalidPackage", "manifest.json"), "definitely not some json");

            IEnumerable<Package> packages = this.PackageList.Get(0, 1);

            // the valid manifest should still be in the list
            Assert.Equal("package_one", packages.First().Id);
            Assert.Single(packages);
        }
    }
}
