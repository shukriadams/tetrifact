using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    public class FileIdentifierTests
    {
        [Fact]
        public void HappyPath()
        {
            FileIdentifier identifier = FileIdentifier.Decloak(Obfuscator.Cloak("myPath::myHash"));
            Assert.Equal("myPath", identifier.Path);
            Assert.Equal("myHash", identifier.Hash);
        }

        [Fact]
        public void GracefulNoBase64Encoding()
        {
            try
            {
                FileIdentifier.Decloak("an-inproperly-formatted-id");
                // should not reach here
                Assert.True(false);
            }
            catch(InvalidFileIdentifierException)
            {
                // should reach here
                Assert.True(true);
            }
        }

        [Fact]
        public void GracefulInvalidContent()
        {
            try
            {
                FileIdentifier.Decloak(Obfuscator.Cloak("an-inproperly-formatted-id"));
                // should not reach here
                Assert.True(false);
            }
            catch (InvalidFileIdentifierException)
            {
                // should reach here
                Assert.True(true);
            }
        }

    }
}
