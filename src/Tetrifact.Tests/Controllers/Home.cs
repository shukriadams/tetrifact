using Xunit;
using Ninject;

namespace Tetrifact.Tests.Controllers
{
    public class Home : TestBase
    {
        private readonly Web.HomeController _controller;

        public Home()
        {
            _controller = this.Kernel.Get<Web.HomeController>();
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
