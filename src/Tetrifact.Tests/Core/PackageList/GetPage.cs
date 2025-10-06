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
            ISettings settings = TestContext.Get<ISettings>();
            IPackageListService packageList = TestContext.Get<IPackageListService>();

            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package2003"));
            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package2002"));
            Directory.CreateDirectory(Path.Combine(settings.PackagePath, "package2001"));

            File.WriteAllText(Path.Combine(settings.PackagePath, "package2003", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            File.WriteAllText(Path.Combine(settings.PackagePath, "package2002", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));
            File.WriteAllText(Path.Combine(settings.PackagePath, "package2001", "manifest.json"), JsonConvert.SerializeObject(new Manifest()));

            Assert.Equal("package2001", packageList.GetPage(0, 1).Items.First().Id);
            Assert.Equal("package2002", packageList.GetPage(1, 1).Items.First().Id);
            Assert.Equal("package2003", packageList.GetPage(2, 1).Items.First().Id);

            PageableData<Package> page = packageList.GetPage(0, 1);
            Assert.Equal(3, page.VirtualItemCount);
            Assert.Equal(3, page.TotalPages);
            Assert.Equal(1, page.PageSize);
            Assert.Equal(0, page.PageIndex);
        }
    }
}
