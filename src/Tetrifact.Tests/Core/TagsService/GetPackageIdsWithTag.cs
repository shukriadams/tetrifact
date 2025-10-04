using Xunit;
using System.Collections.Generic;
using System.Linq;
using Tetrifact.Core;
using Moq;

namespace Tetrifact.Tests.TagsService
{
    public class GetPackageIdsWithTag : Base 
    {
        [Fact]
        public void Happy_path(){

            ITagsService tagsService = TestContext.Get<ITagsService>();

            string[] tags = new [] { "mytag" };

            TestPackage package1 = PackageHelper.CreateNewPackageFiles("package1");
            TestPackage package2 = PackageHelper.CreateNewPackageFiles("package2");

            foreach (string tag in tags) {
                tagsService.AddTag(package1.Id, tag);
                tagsService.AddTag(package2.Id, tag);
            }

            IEnumerable<string> packageIds = tagsService.GetPackageIdsWithTags(tags);

            Assert.Equal(2, packageIds.Count());
            Assert.Contains("package1", packageIds);
            Assert.Contains("package2", packageIds);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Directory_Exception()
        {
            Mock<TestFileSystem> fs = MoqHelper.Mock<TestFileSystem>();
            fs
                .Setup(r => r.Directory.Exists(It.IsAny<string>()))
                .Returns(false);

            ITagsService tagservice = TestContext.Get<ITagsService>("fileSystem", fs.Object);

            Assert.Throws<TagNotFoundException>(()=>{ tagservice.GetPackageIdsWithTags(new [] { "tag123" }); });
        }
    }
}
