using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class FileHelperTests
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
            DiskUseStats stats = FileHelper.GetDiskUseSats();

            // We don't really care about specific values, this test is mostly for coverage, and
            // as longs as values are greater than zero, stats is reading _something_
            Assert.True(stats.FreeBytes > 0);
            Assert.True(stats.TotalBytes > 0);
        }

        [Fact]
        public void RemoveFirstDirectoryFromPath() 
        {
            string path = FileHelper.RemoveFirstDirectoryFromPath("/path/to/test");
            Assert.Equal("to/test", path);
        }
    }
}
