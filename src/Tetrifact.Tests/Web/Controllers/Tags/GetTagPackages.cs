using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Tags
{
    public class GetTagPackages : TestBase
    {
        [Fact]
        public void Happy_path()
        {
            Mock<ITagsService> tagsService = new Mock<ITagsService>();
            tagsService
                .Setup(r => r.GetPackageIdsWithTags(It.IsAny<string[]>()))
                .Returns(new string[] { });

            TagsController controller = NinjectHelper.Get<TagsController>(base.Settings, "tagsService", tagsService.Object);
            JsonResult result = controller.GetTagPackages("tag-list") as JsonResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Unknown_exception()
        {
            Mock<ITagsService> tagsService = new Mock<ITagsService>();
            tagsService
                .Setup(r => r.GetPackageIdsWithTags(It.IsAny<string[]>()))
                .Throws(new Exception());

            TagsController controller = NinjectHelper.Get<TagsController>(base.Settings, "tagsService", tagsService.Object);
            BadRequestObjectResult result = controller.GetTagPackages("tag-list") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
