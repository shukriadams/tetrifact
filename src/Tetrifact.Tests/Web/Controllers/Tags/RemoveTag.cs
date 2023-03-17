using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Tags
{
    public class RemoveTag : TestBase
    {
        [Fact]
        public void Happy_path()
        {
            Mock<ITagsService> tagsService = new Mock<ITagsService>();

            TagsController controller = NinjectHelper.Get<TagsController>(this.Settings, "tagsService", tagsService.Object);
            JsonResult result = controller.RemoveTag("tag", "package-id") as JsonResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Package_not_found()
        {
            Mock<ITagsService> tagsService = new Mock<ITagsService>();
            tagsService
                .Setup(r => r.RemoveTag(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PackageNotFoundException("package-id"));

            TagsController controller = NinjectHelper.Get<TagsController>(this.Settings, "tagsService", tagsService.Object);
            NotFoundObjectResult result = controller.RemoveTag("tag", "package-id") as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Unknown_exception()
        {
            Mock<ITagsService> tagsService = new Mock<ITagsService>();
            tagsService
                .Setup(r => r.RemoveTag(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            TagsController controller = NinjectHelper.Get<TagsController>(this.Settings, "tagsService", tagsService.Object);
            BadRequestObjectResult result = controller.RemoveTag("tag", "package-id") as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
