using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class FileHelperTests : TestBase
    {
        [Fact]
        public void BytesToMegabytes()
        {
            double result = FileHelper.BytesToMegabytes(10000000);
            Assert.Equal(10, result);
        }

        [Fact]
        public void GetDiskUseSats()
        {
            IIndexReadService indexReadServices = NinjectHelper.Get<IIndexReadService>(base.Settings);
            DiskUseStats stats = indexReadServices.GetDiskUseSats();

            // We don't really care about specific values, this test is mostly for coverage, and
            // as longs as values are greater than zero, stats is reading _something_
            Assert.True(stats.FreeBytes > 0);
            Assert.True(stats.TotalBytes > 0);
        }

        /// <summary>
        /// Default behaviour
        /// </summary>
        [Fact]
        public void RemovesFirstDirectoryFromPath() 
        {
            string path = FileHelper.RemoveFirstDirectoryFromPath("/path/to/test");
            Assert.Equal("to/test", path);
        }

        /// <summary>
        /// Ensures that no leading slash is also handled
        /// </summary>
        [Fact]
        public void RemoveFirstDirectoryFromPath_NoLeadingSlash()
        {
            string path = FileHelper.RemoveFirstDirectoryFromPath("path/to/test");
            Assert.Equal("to/test", path);
        }

        /// <summary>
        /// Coverage. Handles a path with no slashes
        /// </summary>
        [Fact]
        public void RemoveFirstDirectoryFromPath_NoSlashes()
        {
            string path = FileHelper.RemoveFirstDirectoryFromPath("no-slashes-string");
            Assert.Equal("no-slashes-string", path);
        }
    }
}
