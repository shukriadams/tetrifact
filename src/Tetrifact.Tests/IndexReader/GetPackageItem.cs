using System.IO;
using System.Text;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageItem : FileSystemBase
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

            Stream stream = this.IndexReader.GetFile("some-project", FileIdentifier.Cloak("path/to/file", hash)).Content;
            byte[] testContent = Core.StreamsHelper.StreamToByteArray(stream);
            Assert.Equal(content, testContent);
        }

        [Fact]
        public void GetInvalidNoProjectInit()
        {
            Assert.Throws<ProjectNotFoundException>(() => this.IndexReader.GetFile("some-project", FileIdentifier.Cloak("invalid/path/to/file", "invalid hash")));
        }

        [Fact]
        public void GetInvalidPackageAndPath()
        {
            this.InitProject();
            Assert.Throws<Core.FileNotFoundException>(() => this.IndexReader.GetFile("some-project", FileIdentifier.Cloak("invalid/path/to/file", "invalid hash")));
        }

    }
}
