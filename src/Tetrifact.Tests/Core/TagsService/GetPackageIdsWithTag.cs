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
            string[] tags = new [] { "mytag" };

            TestPackage package1 = PackageHelper.CreateNewPackageFiles(this.Settings, "package1");
            TestPackage package2 = PackageHelper.CreateNewPackageFiles(this.Settings, "package2");

            foreach (string tag in tags) {
                base.TagsService.AddTag(package1.Id, tag);
                base.TagsService.AddTag(package2.Id, tag);
            }

            IEnumerable<string> packageIds = base.TagsService.GetPackageIdsWithTags(tags);

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
            Mock<TestFileSystem> fs = MockRepository.Create<TestFileSystem>();
            fs
                .Setup(r => r.Directory.Exists(It.IsAny<string>()))
                .Returns(false);

            ITagsService tagservice = NinjectHelper.Get<ITagsService>(base.Settings, "fileSystem", fs.Object);

            Assert.Throws<TagNotFoundException>(()=>{ tagservice.GetPackageIdsWithTags(new [] { "tag123" }); });
        }
    }
}
