using Xunit;

namespace Tetrifact.Tests.StringHelper
{
    [Collection("Tests")]
    public class ClipFromEnd
    {
        [Fact]
        public void Basic() 
        {
            Assert.Equal("98765", Core.StringHelper.ClipFromEnd("9876543210", 5));
        }

        [Fact]
        public void None()
        {
            Assert.Equal("9876543210", Core.StringHelper.ClipFromEnd("9876543210", 0));
        }

        [Fact]
        public void All()
        {
            Assert.Equal(string.Empty, Core.StringHelper.ClipFromEnd("9876543210", 10));
        }
    }
}
