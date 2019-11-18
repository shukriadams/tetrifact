using Tetrifact.Web;
using System.Collections.Generic;
using Xunit;
using Ninject;
using Tetrifact.Core;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Tests.Controllers
{
    public class Home : FileSystemBase
    {
        private readonly Tetrifact.Web.HomeController _controller;

        public Home()
        {
            _controller = this.Kernel.Get<Tetrifact.Web.HomeController>();

            TestingWorkspace.Reset();
        }

        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Touch()
        {
            base.InitProject();
            var result = _controller.Index("some-project");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ContentSummaryModel>(viewResult.ViewData.Model);
            Assert.Equal("some-project", model.CurrentProject);
        }
        
    }
}
