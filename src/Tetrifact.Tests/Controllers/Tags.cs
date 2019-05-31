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

namespace Tetrifact.Tests.Controlers
{
    public class Tags : TestBase
    {
        private readonly Tetrifact.Web.TagsController _controller;

        public Tags()
        {
            _controller = this.Kernel.Get<Tetrifact.Web.TagsController>();

            TestingWorkspace.Reset();
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
