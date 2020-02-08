using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    [Collection("Tests")]
    public class Decloak
    {
        /// <summary>
        /// Can decloak a cloaked string.
        /// </summary>
        [Fact]
        public void CloakThenDecloak()
        {
            // cloak
            string package = "mypackage";
            string path = "mypath";
            string cloaked = Core.FileIdentifier.Cloak(package, path);

            // decloak
            Core.FileIdentifier identifier = Core.FileIdentifier.Decloak(cloaked);
            
            // confirm
            Assert.Equal(package, identifier.Package);
            Assert.Equal(path, identifier.Path);
        }

        /// <summary>
        /// Throws expected exception when fed a string that is not base64 encoded.
        /// </summary>
        [Fact]
        public void GracefulNoBase64Encoding()
        {
            Assert.Throws<InvalidFileIdentifierException>(() =>
            {
                Core.FileIdentifier.Decloak("an-inproperly-formatted-id");
            });
        }

        /// <summary>
        /// Throws expected exception when fed invalid cloaked string.
        /// </summary>
        [Fact]
        public void GracefulInvalidContent()
        {
            Assert.Throws<InvalidFileIdentifierException>(()=>{
                Core.FileIdentifier.Decloak(Obfuscator.Cloak("an-inproperly-formatted-id"));
            });
        }

    }
}
