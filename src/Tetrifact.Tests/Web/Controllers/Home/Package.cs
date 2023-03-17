using Microsoft.AspNetCore.Mvc;
using Moq;
using Tetrifact.Core;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class Package : FileSystemBase
    {
        [Fact]
        public void Index_happy_path()
        {
            Mock<IIndexReadService> packageList = new Mock<IIndexReadService>();
            packageList
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Returns(new Manifest());

            W.HomeController controller = NinjectHelper.Get<W.HomeController>(this.Settings, "indexService", packageList.Object, "settings", Settings);

            ViewResult result = controller.Package("any-id", 1) as ViewResult;
            Assert.NotNull(result);
        }

        // coverage only
        [Fact]
        public void Handle_404()
        {
            Mock<IIndexReadService> packageList = new Mock<IIndexReadService>();
            packageList
                // return ull
                .Setup(r => r.GetManifest(It.IsAny<string>()));

            W.HomeController controller = NinjectHelper.Get<W.HomeController>(this.Settings, "indexService", packageList.Object, "settings", Settings);

            controller.Package("any-id", 0);
        }

    }
}
