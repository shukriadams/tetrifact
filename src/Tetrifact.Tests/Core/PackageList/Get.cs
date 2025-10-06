using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class Get : Base
    {
        [Fact]
        public void Basic()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IPackageListService packageList = TestContext.Get<IPackageListService>();

            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package2003"));
            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package2002"));
            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package2001"));

            File.WriteAllText(Path.Combine(settings.PackagePath, "package2003", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            File.WriteAllText(Path.Combine(settings.PackagePath, "package2002", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            File.WriteAllText(Path.Combine(settings.PackagePath, "package2001", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));

            Assert.Equal("package2001", packageList.Get(0, 1).First().Id);
            Assert.Equal("package2002", packageList.Get(1, 1).First().Id);
            Assert.Equal("package2003", packageList.Get(2, 1).First().Id);
        }

        /// <summary>
        /// Confirms graceful handling of packages with corrupt json
        /// </summary>
        [Fact]
        public void GracefullyHandleInvalidJSON()
        {
            ISettings settings = TestContext.Get<ISettings>();
            TestLogger<IPackageListService> packageListLogger = new TestLogger<IPackageListService>();

            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package_one"));
            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "invalidPackage"));

            File.WriteAllText(Path.Combine(settings.PackagePath, "package_one", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            // write a manifest file that consists of invalid JSON
            File.WriteAllText(Path.Combine(settings.PackagePath, "invalidPackage", "manifest.json"), "definitely not some json");

            PackageListService packageList = MoqHelper.CreateInstanceWithDependencies<PackageListService>(new object[] { packageListLogger });
            IEnumerable<Package> packages = packageList.Get(0, 1);

            // the valid manifest should still be in the list
            Assert.Equal("package_one", packages.First().Id);
            Assert.Single(packages);
            Assert.Single(packageListLogger.LogEntries);
        }
    }
}
