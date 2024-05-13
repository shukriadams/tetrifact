﻿using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    public class FileIdentifierTests : TestBase
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
