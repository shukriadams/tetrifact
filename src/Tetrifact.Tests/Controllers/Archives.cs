using Xunit;
using Ninject;

namespace Tetrifact.Tests.Controllers
{
    public class Archives : TestBase
    {
        private readonly Web.ArchivesController _controller;

        public Archives()
        {
            _controller = this.Kernel.Get<Web.ArchivesController>();

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

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void GetArchiveStatus()
        { 
            dynamic result = JsonHelper.ToDynamic(_controller.GetArchiveStatus("invalid-package"));
            Assert.Null(result.success);
            Assert.NotNull(result.error);
        }
    }
}
