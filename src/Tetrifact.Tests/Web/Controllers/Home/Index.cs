using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using Tetrifact.Core;
using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class Index
    {
        private TestContext _testContext = new TestContext();

         /// <summary>
        /// Confirms that the controller initialized and can be called.
        /// </summary>
        [Fact]
        public void Index_happy_path()
        {
            Mock<IPackageListService> packageList = new Mock<IPackageListService>();
            packageList
                .Setup(r => r.Get(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<Tetrifact.Core.Package>() { });

            packageList
                .Setup(r => r.GetPopularTags(It.IsAny<int>()))
                .Returns(new List<string>() { });

            W.HomeController controller = _testContext.Get<W.HomeController>("packageList", packageList.Object);
            
            ViewResult result = controller.Summary() as ViewResult; 
            Assert.NotNull(result);
        }
    }
}
