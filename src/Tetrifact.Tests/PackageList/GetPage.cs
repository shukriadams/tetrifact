using Newtonsoft.Json;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetPage : Base
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

            Assert.Equal("package2001", this.PackageList.GetPage("some-project", 0, 1).Page.First().Id);
            Assert.Equal("package2002", this.PackageList.GetPage("some-project", 1, 1).Page.First().Id);
            Assert.Equal("package2003", this.PackageList.GetPage("some-project", 2, 1).Page.First().Id);

            PageableData<Package> page = this.PackageList.GetPage("some-project", 0, 1);
            Assert.Equal(3, page.VirtualItemCount);
            Assert.Equal(3, page.TotalPages);
            Assert.Equal(1, page.PageSize);
            Assert.Equal(0, page.PageIndex);
        }
    }
}
