using System.IO;
using System.Text;
using Xunit;
using Tetrifact.Core;
using Tetrifact.Dev;
using System.Collections.Generic;

namespace Tetrifact.Tests.IndexReader
{
    [Collection("Tests")]
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

        /// <summary>
        /// Creates multiple packages with a single file, changing file contents per package. Retrieves the file from each version. Ensures that file is rehydrated from patches.
        /// </summary>
        [Fact]
        public void Rehydration()
        {
            string contentBase = "some content";
            string content = "";
            int steps = 5;

            for (int i = 0; i < steps; i++)
            {
                content = content + contentBase;
                Stream fileStream = StreamsHelper.StreamFromString(content);

                PackageCreate.Create(new PackageCreateArguments
                {
                    Id = $"my package{i}",
                    Project = "some-project",
                    Files = new List<PackageCreateItem>() { (new PackageCreateItem(fileStream, "folder/file")) }
                });
            }


            string expectedContent = "";
            for (int i = 0; i < steps; i++)
            {
                expectedContent = expectedContent + contentBase;
                GetFileResponse response = IndexReader.GetFile("some-project", Core.FileIdentifier.Cloak($"my package{i}", "folder/file"));
                using (StreamReader reader = new StreamReader(response.Content))
                {
                    string retrievedContent = reader.ReadToEnd();
                    Assert.Equal(expectedContent, retrievedContent);
                }
            }
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
