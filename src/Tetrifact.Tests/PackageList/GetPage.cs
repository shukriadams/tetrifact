using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    [Collection("Tests")]
    public class GetPage : Base
    {
        [Fact]
        public void Basic()
        {
            this.CreatePackage("package2003");
            this.CreatePackage("package2002");
            this.CreatePackage("package2001");

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
