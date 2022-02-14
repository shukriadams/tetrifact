using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Controllers
{
    public class Home : TestBase
    {
        private readonly W.HomeController _controller;

        public Home()
        {
            _controller = NinjectHelper.Get<W.HomeController>();
        }

        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Touch()
        {
            _controller.Index();
        }
    }
}
