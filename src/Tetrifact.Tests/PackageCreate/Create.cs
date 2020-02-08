using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;
using Tetrifact.Dev;

namespace Tetrifact.Tests.PackageCreate
{
    [Collection("Tests")]
    public class Create : FileSystemBase
    {
        [Fact]
        public void CreateBasic()
        {
            List<PackageCreateItem> files = new List<PackageCreateItem>();
            string fileContent = "some file content";
            int filesToAdd = 10;
            string packageId = "my package";

            for (int i = 0; i < filesToAdd; i++)
            {
                Stream fileStream = StreamsHelper.StreamFromString(fileContent);
                files.Add(new PackageCreateItem(fileStream,  $"folder{i}/file{i}"));
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
            Package manifest = IndexReader.GetPackage("some-project", packageId);
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
            Assert.Empty(Directory.GetDirectories(Settings.TempPath));
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
            // create first package
            PackageCreate.Create(new PackageCreateArguments
            {
                Id = "my package1",
                Project = "some-project",
                Files = FormFileHelper.Single("content", "folder/file") 
            });

            // create second package
            PackageCreateResult result = PackageCreate.Create(new PackageCreateArguments
            {
                Id = "my package2",
                Project = "some-project",
                BranchFrom = "my package1",
                Files = FormFileHelper.Single("content", "folder/file")
            });

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

            PackageCreateException ex = Assert.Throws<PackageCreateException>(() => PackageCreate.Create(args));
            Assert.Equal(PackageCreateErrorTypes.MissingValue, ex.ErrorType);
            Assert.Equal("Files collection is empty.", ex.PublicError);
        }

        [Fact]
        public void CreateWithEmptyFiles(){

            PackageCreateArguments args = new PackageCreateArguments
            {
                // empty files list
                Files = new List<PackageCreateItem>()
            };

            PackageCreateException ex = Assert.Throws<PackageCreateException>(() => PackageCreate.Create(args));
            Assert.Equal(PackageCreateErrorTypes.MissingValue, ex.ErrorType);
            Assert.Equal("Files collection is empty.", ex.PublicError);
        }        

        [Fact]
        public void CreateWithNoName(){
            PackageCreateArguments args = new PackageCreateArguments();
            args.Files = FormFileHelper.Single("somt text", "folder/file");

            PackageCreateException ex = Assert.Throws<PackageCreateException>(() => PackageCreate.Create(args));
            Assert.Equal(PackageCreateErrorTypes.MissingValue, ex.ErrorType);
            Assert.Equal("Id is required.", ex.PublicError);
        }   

        [Fact]
        public void CreateDuplicatePackage()
        {
            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = "my package",
                Project = "some-project",
                Files = FormFileHelper.Single("some text", "folder/file") 
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.True(result.Success);

            // attempt to create package with same name
            PackageCreateException ex = Assert.Throws<PackageCreateException>(() => PackageCreate.Create(package));
            Assert.Equal(PackageCreateErrorTypes.PackageExists, ex.ErrorType);
        }

        [Fact]
        public void CreateArchiveWithTooManyFiles()
        {
            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = "my package",
                IsArchive = true,
                Project = "some-project",
                Files = FormFileHelper.Multiple(new List<DummyFile> { 
                    new DummyFile("some text", "folder/file"),
                    new DummyFile("some text", "folder/file")   
                })
            };

            PackageCreateException ex = Assert.Throws<PackageCreateException>(()=> PackageCreate.Create(package));
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, ex.ErrorType);
        }

        [Fact]
        public void CreateInvalidArchiveFormat()
        {
            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = "my package",
                Project = "some-project",
                IsArchive = true,
                Files = FormFileHelper.Single("some text", "folder/file")
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidArchiveFormat, result.ErrorType);
        }
    }
}
