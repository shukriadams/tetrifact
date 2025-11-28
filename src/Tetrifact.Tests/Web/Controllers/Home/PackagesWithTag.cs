using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using Tetrifact.Core;
using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class PackagesWithTag
    {
        private TestContext _testContext = new TestContext();

        [Fact]
        public void Happy_path()
        {
            Mock<IPackageListService> packageList = new Mock<IPackageListService>();
            packageList
                .Setup(r => r.GetWithTags(It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<Tetrifact.Core.Package>());

            W.HomeController controller = _testContext.Get<W.HomeController>("packageList", packageList.Object);

            ViewResult result = controller.PackagesWithTag("any-tag") as ViewResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Tag_not_found()
        {
            Mock<IPackageListService> packageList = new Mock<IPackageListService>();
            packageList
                .Setup(r => r.GetWithTags(It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new TagNotFoundException());

            W.HomeController controller = _testContext.Get<W.HomeController>("packageList", packageList.Object);

            controller.PackagesWithTag("any-tag");
        }

    }
}
