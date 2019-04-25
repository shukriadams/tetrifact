using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageCreate
{
    public class Create : Base
    {
        [Fact]
        public void CreateBasic()
        {
            List<IFormFile> files = new List<IFormFile>();
            string fileContent = "some file content";
            int filesToAdd = 10;
            string packageId = "my package";

            for (int i = 0; i < filesToAdd; i++)
            {
                Stream fileStream = StreamsHelper.StreamFromString(fileContent);
                files.Add(new FormFile(fileStream, 0, fileStream.Length, "Files", $"folder{i}/file{i}"));
            }

            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = packageId,
                Files = files
            };

            PackageCreate.CreatePackage(package);

            // check that package can be listed
            IEnumerable<string> packageIds = IndexReader.GetAllPackageIds();
            Assert.Contains(packageId, packageIds);
            Assert.Single(packageIds);

            // check that package can be retrieved as manifest
            Manifest manifest = IndexReader.GetManifest(packageId);
            Assert.NotNull(manifest);
            Assert.Equal(manifest.Files.Count, filesToAdd);

            // check that a file can be retrieved directly using manifest id
            GetFileResponse response = IndexReader.GetFile(manifest.Files[0].Id);
            StreamReader reader = new StreamReader(response.Content);
            string retrievedContent = reader.ReadToEnd();
            Assert.Equal(retrievedContent, fileContent);
        }
    }
}
