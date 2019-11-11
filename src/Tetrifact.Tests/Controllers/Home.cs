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

namespace Tetrifact.Tests.Controllers
{
    public class Home : TestBase
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
            _controller.Index("some-project");
        }
        
    }
}
