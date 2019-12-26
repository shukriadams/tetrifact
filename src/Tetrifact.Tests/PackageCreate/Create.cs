using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;
using Tetrifact.Dev;

namespace Tetrifact.Tests.PackageCreate
{
    public class Create : PackageCreatorBase
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
                Project = "some-project",
                Files = files
            };

            PackageCreateResult result = PackageCreate.Create(package);

            Assert.True(result.Success);
            Assert.Null(result.PublicError);
            Assert.NotEmpty(result.PackageHash);
            Assert.Null(result.ErrorType);

            // check that package can be retrieved as manifest
            Manifest manifest = IndexReader.GetManifest("some-project", packageId);
            Assert.NotNull(manifest);
            Assert.Equal(manifest.Files.Count, filesToAdd);

            // check that a file can be retrieved directly using manifest id
            GetFileResponse response = IndexReader.GetFile("some-project", manifest.Files[0].Id);

            using (StreamReader reader = new StreamReader(response.Content))
            {
                string retrievedContent = reader.ReadToEnd();
                Assert.Equal(retrievedContent, fileContent);
            }

            // ensure that head has been updated - there should be only one head file, and it should contain project id
            Assert.Equal(IndexReader.GetHead("some-project"), packageId);

            // ensure that workspace has been cleaned up
            Assert.Empty(Directory.GetDirectories(base.Settings.TempPath));
        }

        /// <summary>
        /// Head changes to the latest package added
        /// </summary>
        [Fact]
        public void HeadUpdate()
        {
            string fileStream1 = "contentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontent";
            string fileStream2 = "contentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwertycontentcontentcontentcontentqwertyqwertyqwertyqwertyqwertyqwerty";

            // create first package, it should be head
            PackageCreate.Create(new PackageCreateArguments
            {
                Id = "my package1",
                Project = "some-project",
                Files = FormFileHelper.Single(fileStream1, "folder/file")
            });

            Assert.Equal("my package1", IndexReader.GetHead("some-project"));

            // create second package, it should now be head
            PackageCreate.Create(new PackageCreateArguments
            {
                Id = "my package2",
                Project = "some-project",
                Files = FormFileHelper.Single(fileStream2, "folder/file")
            });

            Assert.Equal("my package2", IndexReader.GetHead("some-project"));
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void CreateBranched()
        {
            Stream fileStream = StreamsHelper.StreamFromString("content");

            // create first package
            PackageCreateResult result = PackageCreate.Create(new PackageCreateArguments
            {
                Id = "my package1",
                Project = "some-project",
                Files = new List<IFormFile>() { (new FormFile(fileStream, 0, fileStream.Length, "Files", "folder/file")) }
            });

            Assert.True(result.Success);


            result = PackageCreate.Create(new PackageCreateArguments
            {
                Id = "my package2",
                Project = "some-project",
                BranchFrom = "my package1",
                Files = new List<IFormFile>() { (new FormFile(fileStream, 0, fileStream.Length, "Files", "folder/file")) }
            });

            // create second package
            Assert.True(result.Success);

            // ensure that head has not been updated, as second upload branches from first, and is there not eligable to be head
            Assert.Equal("my package1", IndexReader.GetHead("some-project"));
        }

        /// <summary>
        /// Confirms graceful handling when attempting to create a package with completely empty arguments.
        /// The first check to fail should be file empty check.
        /// </summary>        
        [Fact]
        public void CreateWithNoArguments()
        {

            // empty argument list
            PackageCreateArguments args = new PackageCreateArguments();

            PackageCreateResult result = PackageCreate.Create(args);
            Assert.Equal(PackageCreateErrorTypes.MissingValue, result.ErrorType);
            Assert.Equal("Files collection is empty.", result.PublicError);
        }

        [Fact]
        public void CreateWithEmptyFiles(){

            PackageCreateArguments args = new PackageCreateArguments
            {
                // empty files list
                Files = new List<IFormFile>()
            };

            PackageCreateResult result = PackageCreate.Create(args);
            Assert.Equal(PackageCreateErrorTypes.MissingValue, result.ErrorType);
            Assert.Equal("Files collection is empty.", result.PublicError);
        }        

        [Fact]
        public void CreateWithNoName(){
            PackageCreateArguments args = new PackageCreateArguments();
            Stream fileStream = StreamsHelper.StreamFromString("some text");
            args.Files.Add(new FormFile(fileStream, 0, fileStream.Length, "Files", "folder/file"));


            PackageCreateResult result = PackageCreate.Create(args);
            Assert.Equal(PackageCreateErrorTypes.MissingValue, result.ErrorType);
            Assert.Equal("Id is required.", result.PublicError);
        }   

        [Fact]
        public void CreateDuplicatePackage()
        {
            string packageId = "my package";
            Stream fileStream = StreamsHelper.StreamFromString("some text");

            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = packageId,
                Project = "some-project",
                Files = new List<IFormFile>() {new FormFile(fileStream, 0, fileStream.Length, "Files", "folder/file")}
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.True(result.Success);

            // attempt to create package with same name
            result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.PackageExists, result.ErrorType);
        }

        [Fact]
        public void CreateArchiveWithTooManyFiles()
        {
            string packageId = "my package";
            Stream fileStream = StreamsHelper.StreamFromString("some text");

            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = packageId,
                IsArchive = true,
                Project = "some-project",
                Files = new List<IFormFile>() {
                    new FormFile(fileStream, 0, fileStream.Length, "Files", "folder/file"), 
                    new FormFile(fileStream, 0, fileStream.Length, "Files", "folder/file")
                }
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, result.ErrorType);
        }

        [Fact]
        public void CreateInvalidArchiveFormat()
        {
            string packageId = "my package";
            Stream fileStream = StreamsHelper.StreamFromString("some text");

            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = packageId,
                Project = "some-project",
                IsArchive = true,
                Files = new List<IFormFile>() {
                    new FormFile(fileStream, 0, fileStream.Length, "Files", "folder/file")
                }
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidArchiveFormat, result.ErrorType);
        }
    }
}
