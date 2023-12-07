using Moq;
using System.Collections.Generic;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GeneratePackageData : Base
    {
        /// <summary>
        /// Ensure coverage of manifest not found condition
        /// </summary>
        [Fact]
        public void Manifest_Not_Found()
        {
            // Hit the private GeneratePackageData method by wiping cache.
            MemoryCacheHelper.GetInstance().Remove(Core.PackageListService.CacheKey);

            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();

            // must have a directory of some kind
            mockFileSystem
                .Setup(mq => mq.DirectoryInfo.FromDirectoryName(It.IsAny<string>()).EnumerateDirectories())
                .Returns(new List<IDirectoryInfo> { FileSystem.DirectoryInfo.FromDirectoryName("/some/path"), FileSystem.DirectoryInfo.FromDirectoryName("/some/path2") });

            // force manifest file lookup to find nothing
            mockFileSystem
                .Setup(mq => mq.File.Exists(It.IsAny<string>()))
                .Returns(false);

            // do something to cover manifest file lookup
            this.PackageList = new Core.PackageListService(this.MemoryCache, Settings, new HashService(), TagService, mockFileSystem.Object, this.PackageListLogger);
            this.PackageList.Get(0,1);
        }
    }
}
