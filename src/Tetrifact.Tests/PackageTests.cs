using Tetrifact.Web;
using System.Collections.Generic;
using Xunit;
using Ninject;
using Tetrifact.Core;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.IO.Compression;

namespace Tetrifact.Tests
{
    public class PackageTests : TestBase
    {
        PackagesController _packagesController;
        IPackageService _packageService;

        public PackageTests()
        {
            _packagesController = this.Kernel.Get<PackagesController>();
            _packageService = this.Kernel.Get<IPackageService>();

            TestWorkspaceProvider.Reset();
        }


        [Fact]
        public void GetPackageList()
        {
            // inject 3 indices
            ((TestIndexReader)_packagesController.IndexService).Test_Indexes = new string[] { "1", "2", "3" };

            IEnumerable<string> ids = _packagesController.ListPackages(false, 0, 10).Value as IEnumerable<string>;
            Assert.True(ids.Count() == 3);
        }


        [Fact]
        public void AddPackageAsFiles()
        {
            string file1Content = "file 1 content";
            string file2Content = "file 2 content";
            Stream file1 = StreamsHelper.StreamFromString(file1Content);
            Stream file2 = StreamsHelper.StreamFromString(file2Content);
            string expectedFullhash = HashService.FromString(
                HashService.FromString("folder1/file1.txt") +
                HashService.FromString(file1Content) +
                HashService.FromString("folder2/file2.txt") +
                HashService.FromString(file2Content));

            PackageCreateArguments postArgs = new PackageCreateArguments
            {
                Id = Guid.NewGuid().ToString(),
                Files = new IFormFile[]
                {
                    new FormFile(file1, 0, file1.Length, "Files", "folder1/file1.txt"),
                    new FormFile(file2, 0, file2.Length, "Files", "folder2/file2.txt")
                }
            };

            PackageCreateResult result = _packageService.CreatePackage(postArgs);
            Assert.True(result.Success);
            Assert.Equal(2, TestWorkspaceProvider.Instance.Repository.Count());
            Assert.Empty(TestWorkspaceProvider.Instance.Incoming);
            Assert.Equal(expectedFullhash, result.PackageHash);
        }


        [Fact]
        public void AddPackageAsArchive()
        {
            Dictionary<string, string> files = new Dictionary<string, string>();
            string file1Content = "file 1 content";
            string file2Content = "file 2 content";

            string expectedFullhash = HashService.FromString(
                HashService.FromString("folder1/file1.txt") +
                HashService.FromString(file1Content) +
                HashService.FromString("folder2/file2.txt") +
                HashService.FromString(file2Content));

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
                Format = "zip",
                IsArchive = true,
                Files = new IFormFile[]
                {
                    new FormFile(zipStream, 0, zipStream.Length, "Files", "folder/archive.zip")
                }
            };

            PackageCreateResult result = _packageService.CreatePackage(postArgs);
            Assert.True(result.Success);
            Assert.Equal(2, TestWorkspaceProvider.Instance.Repository.Count());
            Assert.Empty(TestWorkspaceProvider.Instance.Incoming);
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
                Files = new IFormFile[]
                {
                    new FormFile(file, 0, file.Length, "Files", "folder1/file.txt"),
                    new FormFile(file, 0, file.Length, "Files", "folder2/file.txt"),
                }
            };

            PackageCreateResult result = _packageService.CreatePackage(postArgs);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, result.ErrorType);
        }
    }
}
