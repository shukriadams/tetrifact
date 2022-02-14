using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Controllers
{
    public class Files : TestBase
    {
        private readonly W.FilesController _controller;

        public Files()
        {
            _controller = NinjectHelper.Get<W.FilesController>();
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
