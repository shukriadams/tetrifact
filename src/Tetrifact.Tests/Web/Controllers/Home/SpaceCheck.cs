using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class SpaceCheck : TestBase
    {
        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = NinjectHelper.Get<W.HomeController>(base.Settings);
            controller.SpaceCheck();
        }
    }
}
