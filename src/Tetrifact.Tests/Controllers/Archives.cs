using Xunit;
using Ninject;

namespace Tetrifact.Tests.Controllers
{
    [Collection("Tests")]
    public class Archives : TestBase
    {
        private readonly Tetrifact.Web.ArchivesController _controller;

        public Archives()
        {
            _controller = this.Kernel.Get<Tetrifact.Web.ArchivesController>();
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void GetArchive()
        {
            _controller.GetArchive("some-project", "invalid-package");
        }
    }
}
