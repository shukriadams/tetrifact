using System.Collections.Generic;
using Xunit;
using Ninject;
using Tetrifact.Core;
using System.Linq;
using System.IO;
using System;

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

            // create package from zip
            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
            {
                Id = "addZipTest",
                IsArchive = true,
                Project = "some-project",
                Files = FormFileHelper.FromStream(ArchiveHelper.ZipStreamFromFiles(inFiles), "archive.zip")
            });

            // retrieve it all
            Stream outZip = this.IndexReader.GetPackageAsArchive("some-project", "addZipTest");
            IEnumerable<DummyFile> outFiles = ArchiveHelper.FilesFromZipStream(outZip);
            Manifest manifest = this.IndexReader.GetManifest("some-project", "addZipTest");

            // confirm hash in, manifest hash and hash out are equal. If hashes match, binary content + file paths match
            Assert.Equal(manifest.Hash, FormFileHelper.GetHash(inFiles));
            Assert.Equal(manifest.Hash, FormFileHelper.GetHash(outFiles));
        }

        /// <summary>
        /// Creates mulitple packages with same files but changing file content. This tests patching logic.
        /// </summary>
        [Fact]
        public void AddZipLinked() 
        {
            // set up an array of files to use as base
            List<DummyFile> inFiles = new List<DummyFile>();
            for (int i = 0; i < 100; i++)
                inFiles.Add(new DummyFile
                {
                    Data = DataHelper.GetRandomData(1, 100),
                    Path = Guid.NewGuid().ToString()
                });

            int passes = 3;
            for (int i = 0; i < passes; i++) 
            {
                // create package from files array, zipped up
                this.PackageCreate.CreateWithValidation(new PackageCreateArguments
                {
                    Id = $"AddZipLinked{i}",
                    IsArchive = true,
                    Project = "some-project",
                    Files = FormFileHelper.FromStream(ArchiveHelper.ZipStreamFromFiles(inFiles), "archive.zip")
                });

                // retrieve this passes' package and manifest
                Stream outZip = this.IndexReader.GetPackageAsArchive("some-project", $"AddZipLinked{i}");
                IEnumerable<DummyFile> outFiles = ArchiveHelper.FilesFromZipStream(outZip);
                Manifest manifest = this.IndexReader.GetManifest("some-project", $"AddZipLinked{i}");

                // confirm the 3 hashes line up
                Assert.Equal(manifest.Hash, FormFileHelper.GetHash(inFiles));
                Assert.Equal(manifest.Hash, FormFileHelper.GetHash(outFiles));

                // update files for next pass
                foreach (DummyFile file in inFiles)
                    file.Data = DataHelper.GetRandomData(1, 10).Concat(file.Data).ToArray();
            }
        }

        /// <summary>
        /// Fails when trying to create from archive that has more than one file in it
        /// </summary>
        [Fact]
        public void EnsureSingleFileWhenAddArchive()
        {
            PackageCreateArguments postArgs = new PackageCreateArguments
            {
                Id = Guid.NewGuid().ToString(),
                IsArchive = true,
                Files = FormFileHelper.Multiple(new List<DummyFile>() {
                    new DummyFile { Content = "some content", Path = "folder1/file.txt"},
                    new DummyFile { Content = "some content", Path = "folder2/file.txt"}
                })
            };

            PackageCreateResult result = _packageService.CreateWithValidation(postArgs);
            
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, result.ErrorType);
        }
    }
}
