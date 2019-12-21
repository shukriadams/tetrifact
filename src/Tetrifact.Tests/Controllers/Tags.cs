using Xunit;
using Ninject;

namespace Tetrifact.Tests.Controllers
{
    public class Tags : TestBase
    {
        private readonly Web.TagsController _controller;

        public Tags()
        {
            _controller = this.Kernel.Get<Web.TagsController>();
        }

        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Touch()
        {
            _controller.GetTags("some-project");
        }
    }
}
