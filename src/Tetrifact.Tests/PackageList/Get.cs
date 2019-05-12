using Newtonsoft.Json;
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
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package2003"));
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package2002"));
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package2001"));

            File.WriteAllText(Path.Combine(Settings.PackagePath, "package2003", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            File.WriteAllText(Path.Combine(Settings.PackagePath, "package2002", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            File.WriteAllText(Path.Combine(Settings.PackagePath, "package2001", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));

            Assert.Equal("package2001", this.PackageList.Get(0, 1).First().Id);
            Assert.Equal("package2002", this.PackageList.Get(1, 1).First().Id);
            Assert.Equal("package2003", this.PackageList.Get(2, 1).First().Id);
        }
    }
}
