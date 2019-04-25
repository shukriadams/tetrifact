using System.IO;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class CleanRepository : FileSystemBase
    {
        [Fact]
        public void BasicClean()
        {
            // create a file and write to repository using path convention of path/to/file/bin. File is 
            // not linked to 
            string hash = "somehash";
            string path = "some/path/filename.file";
            string content = "file content";
            string rootPath = Path.Combine(base.Settings.RepositoryPath, path, hash);
            Directory.CreateDirectory(rootPath);
            string filePath = Path.Combine(rootPath, "bin");
            File.WriteAllText(filePath, content);

            // ensure file exists
            Assert.True(File.Exists(filePath));

            // assert file is gone after cleaning repo
            IndexReader.CleanRepository();
            Assert.False(File.Exists(filePath));
        }
    }
}
