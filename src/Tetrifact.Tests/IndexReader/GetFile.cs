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
            base.InitProject();

            // create a file and write to repository using path convention of path/to/file/bin
            string hash = "somehash";
            string path = "some/path/filename.file";
            string content = "file content";
            string reposPath = PathHelper.GetExpectedRepositoryPath(base.Settings, "some-project");
            string rootPath =  Path.Combine(reposPath, path, hash);
            Directory.CreateDirectory(rootPath);
            File.WriteAllText(Path.Combine(rootPath, "bin"), content);
            string fileIdentifier = FileIdentifier.Cloak(path, hash);

            GetFileResponse response = IndexReader.GetFile("some-project", fileIdentifier);
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
            Assert.Throws<InvalidFileIdentifierException>(()=> 
            {
                IndexReader.GetFile("some-project", "definitely-an-invalid-file-identifier");
            });
        }

        /// <summary>
        /// Tests graceful handling by GetFile if the project doesn't exist.
        /// </summary>
        [Fact]
        public void GetNonExistentFileFromNonExistentProject()
        {
            string fileIdentifier = FileIdentifier.Cloak("nonexistent/path", "nonexistent-hash");
            ProjectNotFoundException ex = Assert.Throws<ProjectNotFoundException>(() => this.IndexReader.GetFile("some-project", fileIdentifier));
        }

        /// <summary>
        /// Tests graceful handling by GetFile if the file doesn't exist.
        /// </summary>
        [Fact]
        public void GetNonExistentFile()
        {
            string fileIdentifier = FileIdentifier.Cloak("nonexistent/path", "nonexistent-hash");
            Core.Workspace workspace = new Core.Workspace(this.Settings, this.WorkspaceLogger);
            workspace.Initialize("some-project");
            Tetrifact.Core.FileNotFoundException ex = Assert.Throws<Tetrifact.Core.FileNotFoundException>(() => this.IndexReader.GetFile("some-project", fileIdentifier));
        }

    }
}
