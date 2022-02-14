using Xunit;

namespace Tetrifact.Tests.Controllers
{
    public class Clean : TestBase
    {
        private readonly Web.CleanController _controller;

        public Clean()
        {
            _controller = NinjectHelper.Get<Web.CleanController>();
        }

        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Touch()
        {
            _controller.Clean();
        }
        
    }
}
