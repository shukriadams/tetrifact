using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class UploadPackage : TestBase
    {
        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = TestContext.Get<W.HomeController>();
            HttpHelper.EnsureContext(controller);

            ViewResult result = controller.UploadPackage() as ViewResult;
            Assert.NotNull(result);
        }
    }
}
