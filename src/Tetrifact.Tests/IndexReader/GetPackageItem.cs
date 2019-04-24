using System.IO;
using System.Text;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageItem : Base
    {
        [Fact]
        public void GetFile()
        {
            // create package, files folder and item location in one
            byte[] content = Encoding.ASCII.GetBytes("some content");
            string hash = Core.HashService.FromByteArray(content);

            string packageFolder = Path.Combine(this.Settings.RepositoryPath, "path", "to", "file", hash);
            Directory.CreateDirectory(packageFolder);

            File.WriteAllBytes(Path.Join(packageFolder, "bin"), content);

            Stream stream = this.IndexReader.GetFile(Core.FileIdentifier.Cloak("path/to/file", hash)).Content;
            byte[] testContent = Core.StreamsHelper.StreamToByteArray(stream);
            Assert.Equal(content, testContent);
        }

        [Fact]
        public void GetInvalidPackageAndPath()
        {
            Assert.Null(this.IndexReader.GetFile(Core.FileIdentifier.Cloak("invalid/path/to/file", "invalid hash")));
        }

        [Fact]
        public void GetInvalidPath()
        {
            string packageFolder = Path.Combine(this.Settings.PackagePath, "somepackage", "files");
            Directory.CreateDirectory(packageFolder);

            Assert.Null(this.IndexReader.GetFile(Core.FileIdentifier.Cloak("invalid/path/to/file", "invalid hash")));
        }

    }
}
