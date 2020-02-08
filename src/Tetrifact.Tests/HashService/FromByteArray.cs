using System.Text;
using Xunit;

namespace Tetrifact.Tests.HashService
{
    [Collection("Tests")]
    public class FromByteArray : Base
    {
        /// <summary>
        /// Hashing byte array works.
        /// </summary>
        [Fact]
        public void Basic()
        {
            byte[] input = Encoding.ASCII.GetBytes(_input);
            Assert.Equal(_expectedHash, Core.HashService.FromByteArray(input));
        }
    }
}
