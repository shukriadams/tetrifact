using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetFile
    {
        private TestContext _testContext = new TestContext();
        
        [Fact]
        public void Basic()
        {
            ISettings settings = _testContext.Get<ISettings>();
            IIndexReadService indexReader = _testContext.Get<IIndexReadService>();

            // create a file and write to repository using path convention of path/to/file/bin
            string hash = "somehash";
            string path = "some/path/filename.file";
            string content = "file content";
            string rootPath = Path.Combine(settings.RepositoryPath, path, hash);
            Directory.CreateDirectory(rootPath);
            File.WriteAllText(Path.Combine(rootPath, "bin"), content);
            string fileIdentifier = FileIdentifier.Cloak(path, hash);

            GetFileResponse response = indexReader.GetFile(fileIdentifier);
            using (StreamReader reader = new StreamReader(response.Content))
            {
                string retrievedContent = reader.ReadToEnd();
                Assert.Equal(content, retrievedContent);
            }
        }

        /// <summary>
        /// Confirms throwing of proper exception on invalid file identifier. This checks formatting of 
        /// identifier only, not file
        /// </summary>
        [Fact]
        public void GetFileByInvalidIdentifier()
        {
            IIndexReadService indexReader = _testContext.Get<IIndexReadService>();

            Assert.Throws<InvalidFileIdentifierException>(()=> 
            {
                indexReader.GetFile("definitely-an-invalid-file-identifier");
            });
        }

        /// <summary>
        /// Tests graceful handling by GetFile if the file doesn't exist.
        /// </summary>
        [Fact]
        public void GetNonExistentFile()
        {
            IIndexReadService indexReader = _testContext.Get<IIndexReadService>();

            string fileIdentifier = FileIdentifier.Cloak("nonexistent/path", "nonexistent-hash");
            GetFileResponse response = indexReader.GetFile(fileIdentifier);
            Assert.Null(response);
        }
    }
}
