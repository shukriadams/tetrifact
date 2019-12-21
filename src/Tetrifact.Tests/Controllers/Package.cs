using Tetrifact.Web;
using System.Collections.Generic;
using Xunit;
using Ninject;
using System.Linq;

namespace Tetrifact.Tests.Controllers
{
    public class Package : TestBase
    {
        private readonly PackagesController _controller;

        public Package()
        {
            _controller = this.Kernel.Get<PackagesController>();
        }

        [Fact]
        public void GetPackageList()
        {
            // inject 3 indices
            TestIndexReader.Instance.Test_Indexes = new string[] { "1", "2", "3" };

            IEnumerable<string> ids = _controller.ListPackages("some-project", false, 0, 10).Value as IEnumerable<string>;
            Assert.True(ids.Count() == 3);
        }

    }
}
