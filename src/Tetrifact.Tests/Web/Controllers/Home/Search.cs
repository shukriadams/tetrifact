using Microsoft.AspNetCore.Mvc;
using Moq;
using Tetrifact.Core;
using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class Search : TestBase
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Search_happy_path()
        {
            Mock<IPackageListService> packageList = new Mock<IPackageListService>();

             // mock a single search result
            packageList
                .Setup(r => r.Find(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new PageableData<Tetrifact.Core.Package>(new Tetrifact.Core.Package[] { 
                    new Tetrifact.Core.Package()
                    }, 0, 1, 1));

            W.HomeController controller = MoqHelper.CreateInstanceWithDependencies<W.HomeController>(new object[] { this.Settings, packageList });
            ViewResult result = controller.Search("", 0) as ViewResult;
            Assert.NotNull(result);
        }
    }
}
