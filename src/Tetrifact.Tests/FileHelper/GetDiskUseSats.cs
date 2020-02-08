using Xunit;

namespace Tetrifact.Tests.FileHelper
{
    [Collection("Tests")]
    public class GetDiskUseSats
    {
        /// <summary>
        /// DiskUseStats returns an object. Don't have a way to confirm results.
        /// </summary>
        [Fact]
        public void Basic() 
        {
            Core.DiskUseStats stats = Core.FileHelper.GetDiskUseSats();
            Assert.NotNull(stats);
        }
    }
}
