using Xunit;
using Ninject;

namespace Tetrifact.Tests.Controllers
{
    public class Files : TestBase
    {
        private readonly Web.FilesController _controller;

        public Files()
        {
            _controller = this.Kernel.Get<Web.FilesController>();
        }

        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Touch()
        {
            _controller.GetItem("invalid-file-identifier");
        }
    }
}
