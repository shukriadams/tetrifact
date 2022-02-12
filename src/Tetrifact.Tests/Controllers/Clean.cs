using Xunit;
using Ninject;

namespace Tetrifact.Tests.Controlers
{
    public class Clean : TestBase
    {
        private readonly Web.CleanController _controller;

        public Clean()
        {
            _controller = this.Kernel.Get<Web.CleanController>();

            TestingWorkspace.Reset();
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
