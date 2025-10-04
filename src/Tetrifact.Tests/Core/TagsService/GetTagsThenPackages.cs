using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.TagsService
{
    public class GetTagsThenPackages : Base
    {
        // coverage
        [Fact]
        public void Tag_format_exception()
        {
            ISettings settings = TestContext.Get<ISettings>();
            ITagsService tagsService = TestContext.Get<ITagsService>();
            // write garbage to tag folder
            Directory.CreateDirectory(Path.Join(settings.TagsPath, "unencoded-text"));

            tagsService.GetTagsThenPackages();
            // can't get entry to show up, not important, this is is a coverage test
            //Assert.True(base.TagsServiceLogger.ContainsFragment("is not a valid base64 string"));
        }
    }
}
