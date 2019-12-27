using System.IO;
using Xunit;

namespace Tetrifact.Tests.PackageCreate
{
    public class Cleanup : FileSystemBase
    {
        /// <summary>
        /// Temp folder is cleaned after a package is created
        /// </summary>
        [Fact]
        public void OnSuccess()
        {
            // confirm temp is clean
            Assert.Empty(Directory.GetDirectories(Settings.TempPath));
            Assert.Empty(Directory.GetFiles(Settings.TempPath));

            this.CreatePackage();

            // confirm still clean
            Assert.Empty(Directory.GetDirectories(Settings.TempPath));
            Assert.Empty(Directory.GetFiles(Settings.TempPath));
        }

        /// <summary>
        /// Temp folder is cleaned after a package is uploaded but fails to publish
        /// </summary>
        [Fact]
        public void OnFailure()
        {
            // todo : how to do this??
        }

    }
}
