using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetFile : FileSystemBase
    {
        [Fact]
        public void Basic()
        {
            // create a file and write to repository using path convention of path/to/file/bin
            string hash = "somehash";
            string path = "some/path/filename.file";
            string content = "file content";
            string rootPath = Path.Combine(base.Settings.RepositoryPath, path, hash);
            Directory.CreateDirectory(rootPath);
            File.WriteAllText(Path.Combine(rootPath, "bin"), content);
            string fileIdentifier = FileIdentifier.Cloak(path, hash);

            GetFileResponse response = IndexReader.GetFile(fileIdentifier);
            StreamReader reader = new StreamReader(response.Content);
            string retrievedContent = reader.ReadToEnd();
            Assert.Equal(content, retrievedContent);
        }
    }
}
