using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
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
            List<PackageCreateItem> files = new List<PackageCreateItem>();
            string fileContent = "some file content";
            int filesToAdd = 10;
            string packageId = "my package";

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

            PackageCreateResult result = PackageCreate.CreatePackage(package);

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
            PackageCreateItem item = new PackageCreateItem();
        }

        /// <summary>
        /// Confirms graceful handling when attempting to create a package with completely empty arguments.
        /// The first check to fail should be file empty check.
        /// </summary>        
        [Fact]
        public void CreateWithNoArguments(){
            // empty argument list
            PackageCreateArguments args = new PackageCreateArguments();

            PackageCreateResult result = PackageCreate.CreatePackage(args);
            Assert.Equal(PackageCreateErrorTypes.MissingValue, result.ErrorType);
            Assert.Equal("Files collection is empty.", result.PublicError);
        }

        [Fact]
        public void CreateWithEmptyFiles(){

            PackageCreateArguments args = new PackageCreateArguments
            {
                // empty files list
                Files = new List<PackageCreateItem>()
            };

            PackageCreateResult result = PackageCreate.CreatePackage(args);
            Assert.Equal(PackageCreateErrorTypes.MissingValue, result.ErrorType);
            Assert.Equal("Files collection is empty.", result.PublicError);
        }        

        [Fact]
        public void CreateWithNoName(){
            PackageCreateArguments args = new PackageCreateArguments();
            Stream fileStream = StreamsHelper.StreamFromString("some text");
            args.Files.Add(new PackageCreateItem(fileStream, "folder/file"));


            PackageCreateResult result = PackageCreate.CreatePackage(args);
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
                Files = new List<PackageCreateItem>() {new PackageCreateItem(fileStream, "folder/file")}
            };

            PackageCreateResult result = PackageCreate.CreatePackage(package);
            Assert.True(result.Success);

            // attempt to create package with same name
            result = PackageCreate.CreatePackage(package);
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
                Files = new List<PackageCreateItem>() {
                    new PackageCreateItem(fileStream, "folder/file"), 
                    new PackageCreateItem(fileStream, "folder/file")
                }
            };

            PackageCreateResult result = PackageCreate.CreatePackage(package);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, result.ErrorType);
        }

        [Fact]
        public void CreateWithAutoArchive()
        {
            this.Settings.AutoCreateArchiveOnPackageCreate = true;
            Stream fileStream = StreamsHelper.StreamFromString("some text");

            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = "mypackage",
                Files = new List<PackageCreateItem>() {
                    new PackageCreateItem(fileStream, "folder/file")
                }
            };

            PackageCreate.CreatePackage(package);
            Assert.True(File.Exists(Path.Join(Settings.ArchivePath, "mypackage.zip")));
        }
    }
}
