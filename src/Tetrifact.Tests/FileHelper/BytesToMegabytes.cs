using Xunit;

namespace Tetrifact.Tests.FileHelper
{
    [Collection("Tests")]
    public class BytesToMegabytes
    {
        /// <summary>
        /// Returns a single megabytes
        /// </summary>
        [Fact]
        public void Basic() 
        {
            Assert.Equal(1, Core.FileHelper.BytesToMegabytes(1048576));
        }
    }
}
