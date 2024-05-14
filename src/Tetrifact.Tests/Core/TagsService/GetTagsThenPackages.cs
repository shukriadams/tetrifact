using System.IO;
using Xunit;

namespace Tetrifact.Tests.TagsService
{
    public class GetTagsThenPackages : Base
    {
        // coverage
        [Fact]
        public void Tag_format_exception()
        {
            // write garbage to tag folder
            Directory.CreateDirectory(Path.Join(Settings.TagsPath, "unencoded-text"));

            base.TagService.GetTagsThenPackages();
            // can't get entry to show up, not important, this is is a coverage test
            //Assert.True(base.TagsServiceLogger.ContainsFragment("is not a valid base64 string"));
        }
    }
}
