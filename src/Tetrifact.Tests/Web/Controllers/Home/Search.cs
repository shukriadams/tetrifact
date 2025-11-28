using Microsoft.AspNetCore.Mvc;
using Moq;
using Tetrifact.Core;
using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Web.Controllers.Home
{
    public class Search
    {
        private TestContext _testContext = new TestContext();

        private MoqHelper _moqHelper;

        public Search()
        {
            _moqHelper = new MoqHelper(_testContext);
        }

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

            W.HomeController controller = _moqHelper.CreateInstanceWithDependencies<W.HomeController>(new object[] { packageList });
            ViewResult result = controller.Search("", 0) as ViewResult;
            Assert.NotNull(result);
        }
    }
}
