using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class SpaceCheck : TestBase
    {
        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = TestContext.Get<W.HomeController>();
            controller.SpaceCheck();
        }
    }
}
