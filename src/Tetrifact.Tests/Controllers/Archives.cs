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
    public class Archives : TestBase
    {
        private readonly Tetrifact.Web.ArchivesController _controller;

        public Archives()
        {
            _controller = this.Kernel.Get<Tetrifact.Web.ArchivesController>();

            TestingWorkspace.Reset();
        }

        /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Touch()
        {
            _controller.GetArchive("invalid-package");
        }
    }
}
