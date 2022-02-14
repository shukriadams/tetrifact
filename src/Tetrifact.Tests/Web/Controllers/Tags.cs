using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Controllers
{
    public class Tags : TestBase
    {
        private readonly W.TagsController _controller;

        public Tags()
        {
            _controller = NinjectHelper.Get<W.TagsController>();
        }

        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Touch()
        {
            _controller.GetTags();
        }
    }
}
