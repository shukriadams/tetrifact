using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class SpaceCheck
    {
        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = NinjectHelper.Get<W.HomeController>();
            controller.SpaceCheck();
        }
    }
}
