using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageCreate
{
    public class Create : Base
    {
        [Fact]
        public void Happy_Path()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

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
            IEnumerable<string> packageIds = indexReader.GetAllPackageIds();
            Assert.Contains(packageId, packageIds);
            Assert.Single(packageIds);

            // check that package can be retrieved as manifest
            Manifest manifest = indexReader.GetManifest(packageId);
            Assert.NotNull(manifest);
            Assert.Equal(manifest.Files.Count, filesToAdd);

            // check that a file can be retrieved directly using manifest id
            GetFileResponse response = indexReader.GetFile(manifest.Files[0].Id);
            using (StreamReader reader = new StreamReader(response.Content)) { 
                string retrievedContent = reader.ReadToEnd();
                Assert.Equal(retrievedContent, fileContent);
            }

            // ensure that workspace has been cleaned up
            Assert.Empty(Directory.GetDirectories(settings.TempPath));
        }

        [Fact]
        public void CreatePartial() 
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            // create package 1
            List<PackageCreateItem> files1 = new List<PackageCreateItem>();
            string fileContent1 = "some file 1 content";
            int filesToAdd = 5;
            string packageId1 = "my-package1";

            for (int i = 0; i < filesToAdd; i++)
            {
                Stream fileStream = StreamsHelper.StreamFromString(fileContent1);
                files1.Add(new PackageCreateItem(fileStream, $"folder{i}/file{i}"));
            }

            PackageCreate.Create(new PackageCreateArguments
            {
                Id = packageId1,
                Files = files1
            });
            
            Manifest package1Manifest = indexReader.GetManifest(packageId1);

            // create package2, it shares 5 files with package, these will not be uploaded again
            List<PackageCreateItem> files2 = new List<PackageCreateItem>();
            filesToAdd = 5;
            string file2Content = "some file 2 content";
            string packageId2 = "my-package2";

            for (int i = filesToAdd; i < filesToAdd + filesToAdd; i++)
            {
                Stream fileStream = StreamsHelper.StreamFromString(file2Content);
                files2.Add(new PackageCreateItem(fileStream, $"folder{i}/file{i}"));
            }

            PackageCreate.Create(new PackageCreateArguments
            {
                Id = packageId2,
                Files = files2,
                ExistingFiles = new List<ManifestItem>
                {
                    new ManifestItem{Path = package1Manifest.Files[0].Path, Hash = package1Manifest.Files[0].Hash },
                    new ManifestItem{Path = package1Manifest.Files[1].Path, Hash = package1Manifest.Files[1].Hash },
                    new ManifestItem{Path = package1Manifest.Files[2].Path, Hash = package1Manifest.Files[2].Hash },
                    new ManifestItem{Path = package1Manifest.Files[3].Path, Hash = package1Manifest.Files[3].Hash },
                    new ManifestItem{Path = package1Manifest.Files[4].Path, Hash = package1Manifest.Files[4].Hash }
                }
            });

            Manifest package2Manifest = indexReader.GetManifest(packageId2);
            Assert.Equal(10, package2Manifest.Files.Count);
            Assert.Contains<string>("folder0/file0", package2Manifest.Files.Select(r => r.Path));
            Assert.Contains<string>("folder1/file1", package2Manifest.Files.Select(r => r.Path));
            Assert.Contains<string>("folder2/file2", package2Manifest.Files.Select(r => r.Path));
            Assert.Contains<string>("folder3/file3", package2Manifest.Files.Select(r => r.Path));
            Assert.Contains<string>("folder4/file4", package2Manifest.Files.Select(r => r.Path));
            Assert.Contains<string>("folder5/file5", package2Manifest.Files.Select(r => r.Path));
            Assert.Contains<string>("folder6/file6", package2Manifest.Files.Select(r => r.Path));
            Assert.Contains<string>("folder7/file7", package2Manifest.Files.Select(r => r.Path));
            Assert.Contains<string>("folder8/file8", package2Manifest.Files.Select(r => r.Path));
            Assert.Contains<string>("folder9/file9", package2Manifest.Files.Select(r => r.Path));
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
            ISettings settings = TestContext.Get<ISettings>();

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
            Assert.True(File.Exists(Path.Join(settings.RepositoryPath, "folder1/file1.txt", file1Hash, "bin")));
            Assert.True(File.Exists(Path.Join(settings.RepositoryPath, "folder2/file2.txt", file2Hash, "bin")));
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
            ISettings settings = TestContext.Get<ISettings>();
            settings.AutoCreateArchiveOnPackageCreate = true;
            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = "mypackage",
                Files = new List<PackageCreateItem> {
                    new PackageCreateItem(StreamsHelper.StreamFromString("some text"), "folder/file")
                }
            };

            PackageCreate.Create(package);
            IArchiveService archiveService = TestContext.Get<IArchiveService>();
        
            // verify that archiving has been queued, we assume that if this exists, package archiving will be processed later
            string archiveQueuePath = archiveService.GetPackageArchiveQueuePath("mypackage");
            Assert.True(File.Exists(archiveQueuePath));
        }

        [Fact]
        public void CreateDisabled()
        {
            ISettings settings = TestContext.Get<ISettings>();
            settings.PackageCreateEnabled = false;

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
            ISettings settings = TestContext.Get<ISettings>();

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
            Assert.True(File.Exists(Path.Join(settings.RepositoryPath, "folder1/file1.txt", file1Hash, "bin")));
            Assert.True(File.Exists(Path.Join(settings.RepositoryPath, "folder2/file2.txt", file2Hash, "bin")));
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
            IPackageCreateService _packageService = TestContext.Get<IPackageCreateService>();

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
