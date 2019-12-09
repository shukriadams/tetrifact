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
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2003"));
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2002"));
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2001"));

            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2003", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2002", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2001", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));

            Assert.Equal("package2001", this.PackageList.Get("some-project", 0, 1).First().Id);
            Assert.Equal("package2002", this.PackageList.Get("some-project", 1, 1).First().Id);
            Assert.Equal("package2003", this.PackageList.Get("some-project", 2, 1).First().Id);
        }

        /// <summary>
        /// Confirms graceful handling of packages with corrupt json
        /// </summary>
        [Fact]
        public void GracefullyHandleInvalidJSON()
        {
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package_one"));
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "invalidPackage"));

            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package_one", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            // write a manifest file that consists of invalid JSON
            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "invalidPackage", "manifest.json"), "definitely not some json");

            IEnumerable<Package> packages = this.PackageList.Get("some-project", 0, 1);

            // the valid manifest should still be in the list
            Assert.Equal("package_one", packages.First().Id);
            Assert.Single(packages);
            Assert.Single(this.PackageListLogger.LogEntries);
        }
    }
}
