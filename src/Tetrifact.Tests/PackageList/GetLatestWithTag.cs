using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetLatestWithTag : Base
    {
        [Fact]
        public void BasicList()
        {
            // list works by reading manifest json files on system. Create two manifests. All we need are dates on them.
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.PackagesFragment, "package2002"));
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.PackagesFragment, "package2001"));
            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.PackagesFragment, "package2002", "manifest.json"), JsonConvert.SerializeObject(new Manifest() { Tags = new HashSet<string>{ "tag" }, CreatedUtc = DateTime.Parse("2002/1/1") }));
            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.PackagesFragment, "package2001", "manifest.json"), JsonConvert.SerializeObject(new Manifest() { Tags = new HashSet<string> { "tag" }, CreatedUtc = DateTime.Parse("2001/1/1") }));

            Package package = this.PackageList.GetLatestWithTag("some-project", "tag");
            Assert.Equal("package2002", package.Id);
        }
    }
}
