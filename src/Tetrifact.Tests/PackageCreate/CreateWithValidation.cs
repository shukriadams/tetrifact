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

namespace Tetrifact.Tests.PackageCreate
{
    public class CreateWithValidation : FileSystemBase
    {
        private readonly IPackageCreate _packageService;

        public CreateWithValidation()
        {
            _packageService = this.Kernel.Get<IPackageCreate>();
        }

        /// <summary>
        /// Creates a package using a zip archive.
        /// </summary>
        [Fact]
        public void AddZipContent()
        {
            // create a zip stream of 100 files, filled with random binary data
            List<DummyFile> inFiles = new List<DummyFile>();
            for (int i = 0; i < 100; i++)
                inFiles.Add(new DummyFile
                {
                    Data = DataHelper.GetRandomData(1, 100),
                    Path = Guid.NewGuid().ToString()
                });

            inFiles = inFiles.OrderBy(r => r.Path).ToList();

            Stream zipInStream = ArchiveHelper.ZipStreamFromFiles(inFiles);


            // create package from zip
            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
            {
                Id = "addZipTest",
                IsArchive = true,
                Project = "some-project",
                Files = FormFileHelper.FromStream(zipInStream, "archive.zip")
            });

            Stream outZip = this.IndexReader.GetPackageAsArchive("some-project", "addZipTest");

            // confirm retrieved zip contains identical data to infiles collection
            List<DummyFile> outFiles = ArchiveHelper.FilesFromZipStream(outZip).ToList();
            Assert.Equal(outFiles.Count, inFiles.Count);

            outFiles = outFiles.OrderBy(r => r.Path).ToList();

            for (int i = 0; i < inFiles.Count; i++) 
            {
                Assert.Equal(inFiles[i].Path, outFiles[i].Path);
                Assert.Equal(inFiles[i].Data, outFiles[i].Data);
            }
                
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

            PackageCreateResult result = _packageService.CreateWithValidation(postArgs);
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, result.ErrorType);
        }
    }
}
