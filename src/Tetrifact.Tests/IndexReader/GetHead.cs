using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetHead : FileSystemBase
    {
        protected IPackageCreate PackageCreate;

        public GetHead()
        {
            PackageCreate = new Core.PackageCreate(
                IndexReader,
                new TestLogger<IPackageCreate>(),
                new Core.Workspace(this.IndexReader, Settings, new TestLogger<IWorkspace>()));
        }

        [Fact]
        public void Basic()
        {
            this.InitProject();

            Stream fileStream = StreamsHelper.StreamFromString("content");

            // create first package
            Assert.True(PackageCreate.CreatePackage(new PackageCreateArguments
            {
                Id = "my package1",
                Project = "some-project",
                Files = new List<IFormFile>() { (new FormFile(fileStream, 0, fileStream.Length, "Files", $"folder/file")) }
            }).Success);

            // create second package
            Assert.True(PackageCreate.CreatePackage(new PackageCreateArguments
            {
                Id = "my package2",
                Project = "some-project",
                Files = new List<IFormFile>() { (new FormFile(fileStream, 0, fileStream.Length, "Files", $"folder/file")) }
            }).Success);

            Assert.Equal("my package2", IndexReader.GetHead("some-project"));
        }

    }
}
