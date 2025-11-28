using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class Error404
    {
        private TestContext _testContext = new TestContext();

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = _testContext.Get<W.HomeController>();
            controller.Error404();
        }
    }
}
