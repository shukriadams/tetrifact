using Xunit;
using Tetrifact.Core;
using System;

namespace Tetrifact.Tests
{
    public class DiskUseStatsTests
    {
        [Fact]
        public void ToPercent()
        {
            DiskUseStats stats = new DiskUseStats();
            stats.FreeBytes = 50;
            stats.TotalBytes = 100;
            long percent = stats.ToPercent();
            Assert.Equal(50, percent);
        }
    }
}
