using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageCreate
{
    public class Create : Base
    {
        [Fact]
        public void Happy_Path()
        {
            List<PackageCreateItem> files = new List<PackageCreateItem>();
            string fileContent = "some file content";
            int filesToAdd = 10;
            string packageId = "my-package";

            for (int i = 0; i < filesToAdd; i++)
            {
                Stream fileStream = StreamsHelper.StreamFromString(fileContent);
                files.Add(new PackageCreateItem(fileStream, $"folder{i}/file{i}"));
            }

            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = packageId,
                Files = files
            };

            PackageCreateResult result = PackageCreate.Create(package);

            Assert.True(result.Success);
            Assert.Null(result.PublicError);
            Assert.NotEmpty(result.PackageHash);
            Assert.Null(result.ErrorType);

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
            using (StreamReader reader = new StreamReader(response.Content)) { 
                string retrievedContent = reader.ReadToEnd();
                Assert.Equal(retrievedContent, fileContent);
            }

            // ensure that workspace has been cleaned up
            Assert.Empty(Directory.GetDirectories(base.Settings.TempPath));
        }

        /// <summary>
        /// Ensure test coverage of default constructuro
        /// </summary>
        [Fact]
        public void PackageCreateItem_DefaultConstructor_Cover()
        { 
            new PackageCreateItem();
        }

        /// <summary>
        /// Ensure that 
        /// </summary>
        [Fact]
        public void CreateWithEmptyFiles(){

            PackageCreateArguments args = new PackageCreateArguments
            {
                Id = "1234",
                // empty files list
                Files = new List<PackageCreateItem>()
            };

            PackageCreateResult result = PackageCreate.Create(args);
            Assert.Equal(PackageCreateErrorTypes.MissingValue, result.ErrorType);
            Assert.Equal("Files collection is empty.", result.PublicError);
        }        

        [Fact]
        public void CreateWithNoName(){
            PackageCreateArguments args = new PackageCreateArguments();
            Stream fileStream = StreamsHelper.StreamFromString("some text");
            args.Files.Add(new PackageCreateItem(fileStream, "folder/file"));


            PackageCreateResult result = PackageCreate.Create(args);
            Assert.Equal(PackageCreateErrorTypes.MissingValue, result.ErrorType);
            Assert.Equal("Id is required.", result.PublicError);
        }   

        [Fact]
        public void CreateDuplicatePackage()
        {
            string packageId = "my-package";
            Stream fileStream = StreamsHelper.StreamFromString("some text");

            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = packageId,
                Files = new List<PackageCreateItem>() {new PackageCreateItem(fileStream, "folder/file")}
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.True(result.Success);

            // attempt to create package with same name
            result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.PackageExists, result.ErrorType);
        }

        [Fact]
        public void AddPackageAsFiles()
        {
            string file1Content = "file 1 content";
            string file2Content = "file 2 content";
            Stream file1 = StreamsHelper.StreamFromString(file1Content);
            Stream file2 = StreamsHelper.StreamFromString(file2Content);

            IHashService hashService = HashServiceHelper.Instance();

            string file1Hash = hashService.FromString(file1Content);
            string file2Hash = hashService.FromString(file2Content);

            string expectedFullhash = hashService.FromString(
                hashService.FromString("folder1/file1.txt") +
                file1Hash +
                hashService.FromString("folder2/file2.txt") +
                file2Hash);

            PackageCreateArguments postArgs = new PackageCreateArguments
            {
                Id = Guid.NewGuid().ToString(),
                Files = new PackageCreateItem[]
                {
                    new PackageCreateItem(file1, "folder1/file1.txt"),
                    new PackageCreateItem(file2, "folder2/file2.txt")
                }
            };

            PackageCreateResult result = PackageCreate.Create(postArgs);
            Assert.True(result.Success);
            Assert.True(File.Exists(Path.Join(Settings.RepositoryPath, "folder1/file1.txt", file1Hash, "bin")));
            Assert.True(File.Exists(Path.Join(Settings.RepositoryPath, "folder2/file2.txt", file2Hash, "bin")));
            Assert.Equal(expectedFullhash, result.PackageHash);
        }

        [Fact]
        public void CreateArchiveWithTooManyFiles()
        {
            string packageId = "my-package";
            Stream fileStream = StreamsHelper.StreamFromString("some text");

            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = packageId,
                IsArchive = true,
                Files = new List<PackageCreateItem>() {
                    new PackageCreateItem(fileStream, "folder/file"), 
                    new PackageCreateItem(fileStream, "folder/file")
                }
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, result.ErrorType);
        }

        [Fact]
        public void CreateWithAutoArchive()
        {
            this.Settings.AutoCreateArchiveOnPackageCreate = true;
            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = "mypackage",
                Files = new List<PackageCreateItem> {
                    new PackageCreateItem(StreamsHelper.StreamFromString("some text"), "folder/file")
                }
            };

            PackageCreate.Create(package);
            Assert.True(File.Exists(Path.Join(Settings.ArchivePath, "mypackage.zip")));
        }

        [Fact]
        public void CreateDisabled()
        {
            this.Settings.AllowPackageCreate = false;
            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = "mypackage",
                Files = new List<PackageCreateItem> {
                    new PackageCreateItem(StreamsHelper.StreamFromString("some text"), "folder/file")
                }
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.CreateNotAllowed, result.ErrorType);
        }


        [Fact]
        public void AddPackageAsArchive()
        {
            Dictionary<string, string> files = new Dictionary<string, string>();
            string file1Content = "file 1 content";
            string file2Content = "file 2 content";

            IHashService hashService = HashServiceHelper.Instance();
            string file1Hash = hashService.FromString(file1Content);
            string file2Hash = hashService.FromString(file2Content);

            string expectedFullhash = HashServiceHelper.Instance().FromString(
                HashServiceHelper.Instance().FromString("folder1/file1.txt") +
                file1Hash +
                HashServiceHelper.Instance().FromString("folder2/file2.txt") +
                file2Hash);

            files.Add("folder1/file1.txt", file1Content);
            files.Add("folder2/file2.txt", file2Content);

            MemoryStream zipStream = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    ZipArchiveEntry fileEntry = archive.CreateEntry(file.Key);

                    using (var entryStream = fileEntry.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(file.Value);
                    }
                }
            }

            PackageCreateArguments postArgs = new PackageCreateArguments
            {
                Id = Guid.NewGuid().ToString(),
                IsArchive = true,
                Files = new PackageCreateItem[]
                {
                    new PackageCreateItem(zipStream, "folder/archive.zip")
                }
            };

            PackageCreateResult result = PackageCreate.Create(postArgs);
            if (!result.Success)
                throw new Exception(result.PublicError);

            Assert.True(result.Success);
            Assert.True(File.Exists(Path.Join(Settings.RepositoryPath, "folder1/file1.txt", file1Hash, "bin")));
            Assert.True(File.Exists(Path.Join(Settings.RepositoryPath, "folder2/file2.txt", file2Hash, "bin")));
            Assert.Equal(expectedFullhash, result.PackageHash);
        }

        [Fact]
        public void EnsureSingleFileWhenAddArchive()
        {
            
            Stream file = StreamsHelper.StreamFromString("some content");

            PackageCreateArguments postArgs = new PackageCreateArguments
            {
                Id = Guid.NewGuid().ToString(),
                IsArchive = true,
                Files = new PackageCreateItem[]
                {
                    new PackageCreateItem(file, "folder1/file.txt"),
                    new PackageCreateItem(file, "folder2/file.txt"),
                }
            };
            IPackageCreateService _packageService = NinjectHelper.Get<IPackageCreateService>(base.Settings);

            PackageCreateResult result = _packageService.Create(postArgs);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, result.ErrorType);
            
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Handle_No_Files()
        {
            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = "mypackage",
                Files = new List<PackageCreateItem> { }
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.MissingValue, result.ErrorType);
        }

        [Fact]
        public void Handle_No_Id()
        {
            PackageCreateArguments package = new PackageCreateArguments
            {
                Files = new List<PackageCreateItem> {
                    new PackageCreateItem(StreamsHelper.StreamFromString("some text"), "folder/file")
                }
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.MissingValue, result.ErrorType);
        }

        [Fact]
        public void Invalid_name_characters()
        {
            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = "123%*&¤",
                Files = new List<PackageCreateItem> { }
            };

            PackageCreateResult result = PackageCreate.Create(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidName, result.ErrorType);
        }
    }
}
