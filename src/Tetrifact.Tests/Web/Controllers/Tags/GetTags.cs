using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Controllers
{
    public class GetTags : TestBase
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            Mock<ITagsService> tagsService = new Mock<ITagsService>();
            tagsService
                .Setup(r => r.GetAllTags())
                .Returns(new string[] { });

            TagsController controller = NinjectHelper.Get<TagsController>(base.Settings, "tagsService", tagsService.Object);
            JsonResult result = controller.GetTags() as JsonResult;
            Assert.NotNull(result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Unexpected_error()
        {
            Mock<ITagsService> tagsService = new Mock<ITagsService>();
            tagsService
                .Setup(r => r.GetAllTags())
                .Throws(new Exception());

            TagsController controller = NinjectHelper.Get<TagsController>(base.Settings, "tagsService", tagsService.Object);
            BadRequestObjectResult result = controller.GetTags() as BadRequestObjectResult;
            Assert.NotNull(result);
        }
    }
}
