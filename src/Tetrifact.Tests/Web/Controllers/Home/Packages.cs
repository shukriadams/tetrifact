﻿using Microsoft.AspNetCore.Mvc;
using Moq;
using Tetrifact.Core;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class Packages : TestBase
    {
        [Fact]
        public void Happy_path()
        {
            Mock<IPackageListService> packageList = new Mock<IPackageListService>();
            packageList
                .Setup(r => r.GetPage(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new PageableData<Tetrifact.Core.Package>(
                    new Tetrifact.Core.Package[]{ },1,1,1)
                );

            W.HomeController controller = TestContext.Get<W.HomeController>("packageList", packageList.Object, "settings", Settings);

            ViewResult result = controller.Index(1) as ViewResult;
            Assert.NotNull(result);
        }

    }
}
