using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class UploadPackage
    {
        private TestContext _testContext = new TestContext();

        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = _testContext.Get<W.HomeController>();
            HttpHelper.EnsureContext(controller);

            ViewResult result = controller.UploadPackage() as ViewResult;
            Assert.NotNull(result);
        }
    }
}
