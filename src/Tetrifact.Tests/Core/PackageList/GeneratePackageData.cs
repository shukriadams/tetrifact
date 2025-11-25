using Moq;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.Caching.Memory;
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
            IMemoryCache memCach = this.TestContext.Get<IMemoryCache>(); 
            memCach.Remove(Core.PackageListService.CacheKey);
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();
            IPackageListService packageList = TestContext.Get<IPackageListService>();

            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();

            // must have a directory of some kind
            mockFileSystem
                .Setup(mq => mq.DirectoryInfo.FromDirectoryName(It.IsAny<string>()).EnumerateDirectories())
                .Returns(new List<IDirectoryInfo> { fileSystem.DirectoryInfo.FromDirectoryName("/some/path"), fileSystem.DirectoryInfo.FromDirectoryName("/some/path2") });

            // force manifest file lookup to find nothing
            mockFileSystem
                .Setup(mq => mq.File.Exists(It.IsAny<string>()))
                .Returns(false);

            // do something to cover manifest file lookup
            packageList = TestContext.Get<IPackageListService>("fileSystem", mockFileSystem.Object);
            packageList.Get(0,1);
        }
    }
}
