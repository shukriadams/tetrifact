using Tetrifact.Core;
using Xunit;
using Tetrifact.Dev;

namespace Tetrifact.Tests.IndexReader
{
    [Collection("Tests")]
    public class GetManifest : FileSystemBase
    {
        [Fact]
        public void Get()
        {
            // create package
            DummyPackage package = this.CreatePackage();

            Package testManifest = this.IndexReader.GetPackage("some-project", package.Id);
            Assert.Single(testManifest.Files);
            Assert.Equal(package.Files[0].Path, testManifest.Files[0].Path);
        }

        /// <summary>
        /// Handles reading a manifest that doesn't exist.
        /// </summary>
        [Fact]
        public void GetEmpty()
        {
            Package testManifest = this.IndexReader.GetPackage("some-project", "someinvalidpackage");
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
            Package testManifest = this.IndexReader.GetPackage("some-project", "someinvalidpackage");
            Assert.Null(testManifest);
        }
    }
}
