using Xunit;

namespace Tetrifact.Tests.DiskUseStats
{
    public class ToPercent
    {
        /// <summary>
        /// ToPercent returns expected value.
        /// </summary>
        [Fact]
        public void Basic() 
        {
            Core.DiskUseStats stats = new Core.DiskUseStats();
            stats.FreeBytes = 10;
            stats.TotalBytes = 20;
            Assert.Equal(50, stats.ToPercent());
        }
    }
}
