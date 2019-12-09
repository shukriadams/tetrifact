using Tetrifact.Web;
using Xunit;
using Ninject;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Tests.Controllers
{
    public class Home : FileSystemBase
    {
        private readonly HomeController _controller;

        public Home()
        {
            _controller = this.Kernel.Get<HomeController>();

            TestingWorkspace.Reset();
        }

        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Touch()
        {
            base.InitProject();
            var result = _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            ContentSummaryModel model = Assert.IsAssignableFrom<ContentSummaryModel>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Projects.Count());
        }
        
    }
}
