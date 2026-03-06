using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class ToPercent
    {
        [Fact]
        public void HappyPath()
        {
            DiskUseStats stats = new DiskUseStats();
            stats.FreeBytes = 50;
            stats.TotalBytes = 100;
            long percent = stats.ToPercent();
            Assert.Equal(50, percent);
        }
    }
}
