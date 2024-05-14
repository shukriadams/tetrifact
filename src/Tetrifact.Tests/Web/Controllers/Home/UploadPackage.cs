﻿using Microsoft.AspNetCore.Mvc;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class UploadPackage : TestBase
    {
        [Fact]
        public void Happy_path()
        {
            W.HomeController controller = NinjectHelper.Get<W.HomeController>("settings", SettingsHelper.CurrentSettingsContext);
            HttpHelper.EnsureContext(controller);

            ViewResult result = controller.UploadPackage() as ViewResult;
            Assert.NotNull(result);
        }
    }
}
