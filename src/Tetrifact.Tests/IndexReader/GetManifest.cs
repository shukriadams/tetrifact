using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetManifest : FileSystemBase
    {
        [Fact]
        public void Get()
        {
            // create package
            DummyPackage package = this.CreatePackage();

            Manifest testManifest = this.IndexReader.GetManifest("some-project", package.Id);
            Assert.Single(testManifest.Files);
            Assert.Equal(package.Files[0].Path, testManifest.Files[0].Path);
        }

        /// <summary>
        /// Handles reading a manifest that doesn't exist.
        /// </summary>
        [Fact]
        public void GetEmpty()
        {
            Manifest testManifest = this.IndexReader.GetManifest("some-project", "someinvalidpackage");
            Assert.Null(testManifest);

            // should not generate a log message
            Assert.Empty(((TestLogger<IIndexReader>)this.Logger).LogEntries);
        }

        /// <summary>
        /// Handles reading an existing manifest file that isn't a valid json file.
        /// </summary>
        [Fact]
        public void GetInvalidManifet()
        {
            Manifest testManifest = this.IndexReader.GetManifest("some-project", "someinvalidpackage");
            Assert.Null(testManifest);
        }
    }
}
