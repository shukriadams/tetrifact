using System.IO;
using System.Text;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.IndexReader
{
    public class GetFile : FileSystemBase
    {
        /// <summary>
        /// Tests the GetFile method to retrieve the contents of a known binary file.
        /// </summary>
        [Fact]
        public void Basic()
        {
            DummyPackage package = this.CreatePackage("package2");

            // do
            Stream stream = this.IndexReader.GetFile("some-project", Core.FileIdentifier.Cloak(package.Id, package.Files[0].Path)).Content;

            // test
            byte[] testContent = StreamsHelper.StreamToByteArray(stream);
            Assert.Equal(Encoding.ASCII.GetBytes(package.Files[0].Content), testContent);
        }

        [Fact]
        public void GetInvalidNoProjectInit()
        {
            Assert.Throws<ProjectNotFoundException>(() => this.IndexReader.GetFile("inavlid-project", Core.FileIdentifier.Cloak("mypackage", "invalid/path/to/file")));
        }

        [Fact]
        public void GetInvalidNoPackageInit()
        {
            Assert.Throws<PackageNotFoundException>(() => this.IndexReader.GetFile("some-project", Core.FileIdentifier.Cloak("mypackage", "invalid/path/to/file")));
        }

        [Fact]
        public void GetInvalidPath()
        {
            CreatePackage("mypackage");
            Assert.Throws<Core.FileNotFoundException>(() => this.IndexReader.GetFile("some-project", Core.FileIdentifier.Cloak("mypackage", "invalid/path/to/file")));
        }

    }
}
