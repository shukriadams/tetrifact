using Microsoft.AspNetCore.Mvc;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class UploadPackage : FileSystemBase
    {
        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = NinjectHelper.Get<W.HomeController>(base.Settings, "settings", Settings);
            HttpHelper.EnsureContext(controller);

            ViewResult result = controller.UploadPackage() as ViewResult;
            Assert.NotNull(result);
        }
    }
}
