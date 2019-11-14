using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    public class FileIdentifierTests
    {
        [Fact]
        public void HappyPath()
        {
            FileIdentifier identifier = FileIdentifier.Decloak(Obfuscator.Cloak("mypackage::mypath"));
            Assert.Equal("mypackage", identifier.Package);
            Assert.Equal("mypath", identifier.Path);
        }

        [Fact]
        public void GracefulNoBase64Encoding()
        {
            Assert.Throws<InvalidFileIdentifierException>(() =>
            {
               FileIdentifier.Decloak("an-inproperly-formatted-id");
            });
        }

        [Fact]
        public void GracefulInvalidContent()
        {
            Assert.Throws<InvalidFileIdentifierException>(()=>{
                FileIdentifier.Decloak(Obfuscator.Cloak("an-inproperly-formatted-id"));
            });
        }

    }
}
