using Tetrifact.Web;
using Xunit;
using Ninject;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Tetrifact.Tests.Controllers
{
    public class Home : FileSystemBase
    {
        private readonly HomeController _controller;

        public Home()
        {
            _controller = this.Kernel.Get<HomeController>();
        }

        /// <summary>
        /// Index returns a valid render model
        /// </summary>
        [Fact]
        public void Index()
        {
            // set fake projects
            TestIndexReader.Instance.Test_Projects = new List<string>() { "1", "2" };

            // "render"
            IActionResult result = _controller.Index();

            // test
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            ContentSummaryModel model = Assert.IsAssignableFrom<ContentSummaryModel>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Projects.Count());
            Assert.Contains("1", model.Projects);
            Assert.Contains("2", model.Projects);
        }

    }
}
