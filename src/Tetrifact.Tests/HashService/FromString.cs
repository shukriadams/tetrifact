using Xunit;

namespace Tetrifact.Tests.HashService
{
    [Collection("Tests")]
    public class FromString : Base
    {
        /// <summary>
        /// Hashing a string works.
        /// </summary>
        [Fact]
        public void Basic()
        {
            Assert.Equal(_expectedHash, Core.HashService.FromString(_input));
        }
    }
}
