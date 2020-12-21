using Tetrifact.Web;
using System.Collections.Generic;
using Xunit;
using Ninject;
using Tetrifact.Core;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System;
using System.IO.Compression;

namespace Tetrifact.Tests.Controlers
{
    public class Files : TestBase
    {
        private readonly Tetrifact.Web.FilesController _controller;

        public Files()
        {
            _controller = this.Kernel.Get<Tetrifact.Web.FilesController>();

            TestingWorkspace.Reset();
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
