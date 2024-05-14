using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class Find : Base
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Happy_Path()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();

            // need to find 3 directories, one per manifest
            mockFileSystem
                .Setup(mq => mq.DirectoryInfo.FromDirectoryName(It.IsAny<string>()).EnumerateDirectories())
                .Returns(new List<IDirectoryInfo> { 
                    FileSystem.DirectoryInfo.FromDirectoryName("/some/path"), 
                    FileSystem.DirectoryInfo.FromDirectoryName("/some/path2"),
                    FileSystem.DirectoryInfo.FromDirectoryName("/some/path3")
                });

            // manifest files should always exist
            mockFileSystem
                .Setup(mq => mq.File.Exists(It.IsAny<string>()))
                .Returns(true);

            // make 3 manifests, return sequentially per call
            RotatingCollection<string> manifests = new RotatingCollection<string>(new [] {
                JsonConvert.SerializeObject(new Manifest { Id = "1test1", Tags = new HashSet<string>(new []{ "1tag1" })}),
                JsonConvert.SerializeObject(new Manifest { Id = "2test2", Tags = new HashSet<string>(new []{ "2tag2" })}),
                JsonConvert.SerializeObject(new Manifest { Id = "3test3", Tags = new HashSet<string>(new []{ "3tag3" })})
            });

            mockFileSystem
                .Setup(mq => mq.File.ReadAllText(It.IsAny<string>()))
                .Returns(manifests.Next());

            IPackageListService listService = MoqHelper.CreateInstanceWithDependencies<PackageListService>(new object[]{ Settings, mockFileSystem });
            PageableData<Package> results = listService.Find("test", 0, 10);
            Assert.Equal(3, results.VirtualItemCount);
        }
    }
}
