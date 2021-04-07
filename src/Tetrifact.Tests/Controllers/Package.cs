using Tetrifact.Web;
using System.Collections.Generic;
using Xunit;
using Ninject;
using Tetrifact.Core;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System;
using System.IO.Compression;
using System.Dynamic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Tests.Controlers
{
    public class Package : TestBase
    {
        private readonly PackagesController _controller;

        private readonly IPackageCreate _packageService;

        public Package()
        {
            _controller = this.Kernel.Get<PackagesController>();
            _packageService = this.Kernel.Get<IPackageCreate>();

            TestingWorkspace.Reset();
        }

        [Fact]
        public void GetPackageIdList()
        {
            // inject 3 indices
            TestIndexReader.Test_Indexes = new string[] { "1", "2", "3" };
            dynamic json = this.ToDynamic(_controller.ListPackages(false, 0, 10));
            string[] ids = json.success.packages.ToObject<string[]>();
            Assert.Equal(3, ids.Count());
        }

        [Fact]
        public void GetPackageList()
        {
            // inject 3 objects
            TestPackageList.Packages = new List<Core.Package>() { new Core.Package(), new Core.Package(), new Core.Package() };
            dynamic json = this.ToDynamic(_controller.ListPackages(true, 0, 10));
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
            string expectedFullhash = HashService.FromString(
                HashService.FromString("folder1/file1.txt") +
                HashService.FromString(file1Content) +
                HashService.FromString("folder2/file2.txt") +
                HashService.FromString(file2Content));

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
            Assert.Equal(2, TestingWorkspace.Repository.Count());
            Assert.Empty(TestingWorkspace.Incoming);
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
                IsArchive = true,
                Files = new PackageCreateItem[]
                {
                    new PackageCreateItem(zipStream, "folder/archive.zip")
                }
            };

            PackageCreateResult result = _packageService.CreatePackage(postArgs);
            Assert.True(result.Success);
            Assert.Equal(2, TestingWorkspace.Repository.Count());
            Assert.Empty(TestingWorkspace.Incoming);
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
