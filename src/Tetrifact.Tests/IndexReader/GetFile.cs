using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetFile : FileSystemBase
    {
        /// <summary>
        /// Retrieves a file which is directly written to the path files are read from.
        /// </summary>
        [Fact]
        public void Basic()
        {
            base.InitProject();

            // create a file and write to repository using path convention of path/to/file/bin
            string path = "some/path/filename.file";
            string content = "file content";
            string package = "some-package";
            string shardRoot = PathHelper.ResolveShardRoot(base.Settings, "some-project");
            string rootPath =  Path.Combine(shardRoot, package, path);
            Directory.CreateDirectory(rootPath);
            File.WriteAllText(Path.Combine(rootPath, "bin"), content);
            string fileIdentifier = FileIdentifier.Cloak(package, path);

            GetFileResponse response = IndexReader.GetFile("some-project", fileIdentifier);
            using (StreamReader reader = new StreamReader(response.Content))
            {
                string retrievedContent = reader.ReadToEnd();
                Assert.Equal(content, retrievedContent);
            }
        }

        /// <summary>
        /// Retrieves a file as it changes over several packages. This tests both the package create and package 
        /// </summary>
        [Fact]
        public void GetsChangesAcrossPackages()
        { 

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
            string fileIdentifier = FileIdentifier.Cloak("nonexistent-package", "nonexistent/path");
            ProjectNotFoundException ex = Assert.Throws<ProjectNotFoundException>(() => this.IndexReader.GetFile("some-project", fileIdentifier));
        }

        /// <summary>
        /// Tests graceful handling by GetFile if the file doesn't exist.
        /// </summary>
        [Fact]
        public void GetNonExistentFile()
        {
            this.InitProject();
            string fileIdentifier = FileIdentifier.Cloak("nonexistent/package", "nonexistent-path");
            Tetrifact.Core.FileNotFoundException ex = Assert.Throws<Tetrifact.Core.FileNotFoundException>(() => this.IndexReader.GetFile("some-project", fileIdentifier));
        }

    }
}
