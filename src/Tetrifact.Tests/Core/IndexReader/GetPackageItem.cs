using System.IO;
using System.Text;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageItem : FileSystemBase
    {
        [Fact]
        public void GetFile()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            // create package, files folder and item location in one
            byte[] content = Encoding.ASCII.GetBytes("some content");
            string hash = HashServiceHelper.Instance().FromByteArray(content);

            string packageFolder = Path.Combine(settings.RepositoryPath, "path", "to", "file", hash);
            Directory.CreateDirectory(packageFolder);

            File.WriteAllBytes(Path.Join(packageFolder, "bin"), content);

            Stream stream = indexReader.GetFile(FileIdentifier.Cloak("path/to/file", hash)).Content;
            byte[] testContent = Core.StreamsHelper.StreamToByteArray(stream);
            Assert.Equal(content, testContent);
        }

        [Fact]
        public void GetInvalidPackageAndPath()
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            Assert.Null(indexReader.GetFile(Core.FileIdentifier.Cloak("invalid/path/to/file", "invalid hash")));
        }

        [Fact]
        public void GetInvalidPath()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            string packageFolder = Path.Combine(settings.PackagePath, "somepackage", "files");
            Directory.CreateDirectory(packageFolder);

            Assert.Null(indexReader.GetFile(Core.FileIdentifier.Cloak("invalid/path/to/file", "invalid hash")));
        }

    }
}
