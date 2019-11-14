using System.IO;
using System.Text;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageItem : FileSystemBase
    {
        /// <summary>
        /// Tests the GetFile method to retrieve the contents of a known binary file.
        /// </summary>
        [Fact]
        public void GetFile()
        {
            // setup : directly write binary file to disk loation we expect it to be read from
            base.InitProject();
            byte[] content = Encoding.ASCII.GetBytes("some content");
            string package = "my-package";
            string shardFolder = PathHelper.ResolveShardRoot(base.Settings, "some-project");
            string packageFolder = Path.Combine(shardFolder, package, "path", "to", "file");
            Directory.CreateDirectory(packageFolder);
            File.WriteAllBytes(Path.Join(packageFolder, "bin"), content);

            // do
            Stream stream = this.IndexReader.GetFile("some-project", FileIdentifier.Cloak(package, "path/to/file")).Content;

            // test
            byte[] testContent = StreamsHelper.StreamToByteArray(stream);
            Assert.Equal(content, testContent);
        }

        [Fact]
        public void GetInvalidNoProjectInit()
        {
            Assert.Throws<ProjectNotFoundException>(() => this.IndexReader.GetFile("some-project", FileIdentifier.Cloak("mypackage", "invalid/path/to/file")));
        }

        [Fact]
        public void GetInvalidPackageAndPath()
        {
            this.InitProject();
            Assert.Throws<Core.FileNotFoundException>(() => this.IndexReader.GetFile("some-project", FileIdentifier.Cloak("mypackage", "invalid/path/to/file")));
        }

    }
}
