using Moq;
using System.Collections.Generic;
using System.IO.Abstractions;
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
            MemoryCacheHelper.GetInstance().Remove(Core.PackageList.CacheKey);

            IFileSystem mockFileSystem = Mock.Of<IFileSystem>();
            // must have a directory of some kind
            Mock.Get(mockFileSystem)
                .Setup(f => f.DirectoryInfo.FromDirectoryName(It.IsAny<string>()).EnumerateDirectories())
                .Returns(new List<IDirectoryInfo> { FileSystem.DirectoryInfo.FromDirectoryName("/some/path"), FileSystem.DirectoryInfo.FromDirectoryName("/some/path2") });

            // force manifest file lookup to find nothing
            Mock.Get(mockFileSystem)
                .Setup(f => f.File.Exists(It.IsAny<string>()))
                .Returns(false);

            // do something to cover manifest file lookup
            this.PackageList = new Core.PackageList(this.MemoryCache, Settings, TagService, mockFileSystem, this.PackageListLogger);
            this.PackageList.Get(0,1);
        }
    }
}
