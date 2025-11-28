using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class SpaceCheck
    {
        private TestContext _testContext = new TestContext();

        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = _testContext.Get<W.HomeController>();
            controller.SpaceCheck();
        }
    }
}
