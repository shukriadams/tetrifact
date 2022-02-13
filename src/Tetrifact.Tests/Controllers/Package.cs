using Tetrifact.Web;
using System.Collections.Generic;
using Xunit;
using Ninject;
using Tetrifact.Core;
using System.Linq;
using System.IO;
using System;
using System.IO.Compression;
using Ninject.Parameters;
using Moq;

namespace Tetrifact.Tests.Controllers
{
    public class Package : TestBase
    {
        private readonly PackagesController _controller;

        private readonly IPackageCreateService _packageService;

        public Package()
        {
            _controller = this.Kernel.Get<PackagesController>();
            _packageService = this.Kernel.Get<IPackageCreateService>();

            TestingWorkspace.Reset();
        }

        [Fact]
        public void GetPackageIdList()
        {
            // inject 3 indices
            TestIndexReader.Test_Indexes = new string[] { "1", "2", "3" };
            dynamic json = JsonHelper.ToDynamic(_controller.ListPackages(false, 0, 10));
            string[] ids = json.success.packages.ToObject<string[]>();
            Assert.Equal(3, ids.Count());
        }

        [Fact]
        public void GetPackageList()
        {
            IPackageListService moqListService = Mock.Of<IPackageListService>();
            Mock.Get(moqListService)
                .Setup(r => r.Get(It.IsAny<int>(), It.IsAny<int>()))
                .Returns( new List<Core.Package>() {
                    new Core.Package(), new Core.Package(), new Core.Package() // inject 3 packages
                });

            PackagesController controller = this.Kernel.Get<PackagesController>(new ConstructorArgument("packageListService", moqListService) );
            dynamic json = JsonHelper.ToDynamic(controller.ListPackages(true, 0, 10));
            Core.Package[] packages = json.success.packages.ToObject<Core.Package[]>();
            Assert.Equal(3, packages.Count());
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

            PackageCreateResult result = _packageService.CreatePackage(postArgs);
            Assert.True(result.Success);
            Assert.True(File.Exists(Path.Join(Settings.RepositoryPath, "folder1/file1.txt", file1Hash, "bin")));
            Assert.True(File.Exists(Path.Join(Settings.RepositoryPath, "folder2/file2.txt", file2Hash, "bin")));
            Assert.Equal(expectedFullhash, result.PackageHash);
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

            PackageCreateResult result = _packageService.CreatePackage(postArgs);
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

            PackageCreateResult result = _packageService.CreatePackage(postArgs);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, result.ErrorType);
        }
    }
}
