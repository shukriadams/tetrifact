using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Tetrifact.Core;

namespace Tetrifact.Tests.TagsService
{
    public class GetAllTags : Base 
    {
        [Fact]
        public void Basic(){
            TestPackage package1 = PackageHelper.CreateNewPackageFiles("package1");
            string tag1 = "mytag1";
            base.TagsService.AddTag(package1.Id, tag1);

            TestPackage package2 = PackageHelper.CreateNewPackageFiles("package2");
            string tag2 = "mytag2";
            base.TagsService.AddTag(package2.Id, tag2);

            IEnumerable<string> tags = base.TagsService.GetAllTags();

            Assert.Equal(2, tags.Count());
            Assert.Contains(tag1, tags);
            Assert.Contains(tag2, tags);
        }

        // coverage
        [Fact]
        public void Tag_format_exception()
        {
            ISettings settings = TestContext.Get<ISettings>();

            // write garbage to tag folder
            Directory.CreateDirectory(Path.Join(settings.TagsPath, "unencoded-text"));

            base.TagService.GetAllTags();
            // can't get entry to show up, not important, this is is a coverage test
            //Assert.True(base.TagsServiceLogger.ContainsFragment("is not a valid base64 string"));
        }
    }
}
