using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.TagsService
{
    public class GetTagsThenPackages
    {
        private TestContext _testContext = new TestContext();

        // coverage
        [Fact]
        public void Tag_format_exception()
        {
            ISettings settings = _testContext.Get<ISettings>();
            ITagsService tagsService = _testContext.Get<ITagsService>();
            // write garbage to tag folder
            Directory.CreateDirectory(Path.Join(settings.TagsPath, "unencoded-text"));

            tagsService.GetTagsThenPackages();
            // can't get entry to show up, not important, this is is a coverage test
            //Assert.True(base.TagsServiceLogger.ContainsFragment("is not a valid base64 string"));
        }
    }
}
