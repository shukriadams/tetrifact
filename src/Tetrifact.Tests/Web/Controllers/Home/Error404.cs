using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class Error404 : TestBase
    {
        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = TestContext.Get<W.HomeController>();
            controller.Error404();
        }
    }
}
