using Microsoft.AspNetCore.Mvc;
using Moq;
using Tetrifact.Core;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class Package : TestBase
    {
        [Fact]
        public void Index_happy_path()
        {
            ISettings settings = TestContext.Get<ISettings>();

            Mock<IIndexReadService> packageList = new Mock<IIndexReadService>();
            packageList
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Returns(new Manifest());

            W.HomeController controller = TestContext.Get<W.HomeController>("indexService", packageList.Object, "settings", settings);

            ViewResult result = controller.Package("any-id", 1) as ViewResult;
            Assert.NotNull(result);
        }

        // coverage only
        [Fact]
        public void Handle_404()
        {
            ISettings settings = TestContext.Get<ISettings>();

            Mock<IIndexReadService> packageList = new Mock<IIndexReadService>();
            packageList
                // return ull
                .Setup(r => r.GetManifest(It.IsAny<string>()));

            W.HomeController controller = TestContext.Get<W.HomeController>("indexService", packageList.Object, "settings", settings);

            controller.Package("any-id", 0);
        }

    }
}
