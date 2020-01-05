using System.Collections.Generic;
using Xunit;
using Ninject;
using Tetrifact.Core;
using System.Linq;
using System.IO;
using System;
using Tetrifact.Dev;

namespace Tetrifact.Tests.PackageCreate
{
    public class CreateWithValidation : FileSystemBase
    {
        #region CTORS

        private readonly IPackageCreate _packageService;

        #endregion

        #region CTORS

        public CreateWithValidation()
        {
            _packageService = this.Kernel.Get<IPackageCreate>();
        }

        #endregion

        #region TESTS

        [Fact]
        public void AddZipStackedBsDiff() 
        {
            AddZipStacked(DiffMethods.BsDiff, 3);
        }

        [Fact]
        public void AddZipStackedVcDiff()
        {
            AddZipStacked(DiffMethods.VcDiff, 3);
        }

        /// <summary>
        /// Creates mulitple packages with same files but changing file content. This tests patching logic.
        /// </summary>
        private void AddZipStacked(DiffMethods diffMethod, int passes) 
        {
            Settings.DiffMethod = diffMethod;

            // set up an array of files to use as base
            List<DummyFile> inFiles = new List<DummyFile>();
            for (int i = 0; i < 50; i++)
                inFiles.Add(new DummyFile
                {
                    Data = DataHelper.GetRandomData(1, 100),
                    Path = Guid.NewGuid().ToString()
                });

            for (int i = 0; i < passes; i++) 
            {
                // create package from files array, zipped up
                this.PackageCreate.Create(new PackageCreateArguments
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

                // confirm the 3 hashes line up - this confirms file content, paths, count and order at once
                Assert.Equal(manifest.Hash, FormFileHelper.GetHash(inFiles));
                Assert.Equal(manifest.Hash, FormFileHelper.GetHash(outFiles));

                // update files for next pass
                foreach (DummyFile file in inFiles)
                    file.Data = DataHelper.GetRandomData(1, 10).Concat(file.Data).ToArray();
            }
        }

        /// <summary>
        /// Gracefully handles a file that starts empty, then has data, then has none again.
        /// </summary>
        [Fact]
        public void EmptyFileHandlingStartingEmpty() 
        {
            EmptyFileHandling(DiffMethods.VcDiff, true);
        }

        /// <summary>
        /// Gracefully handles a file that starts with data, then gets emptied, then has data again.
        /// </summary>
        [Fact]
        public void EmptyFileHandlingStartingFull()
        {
            EmptyFileHandling(DiffMethods.VcDiff, false);
        }

        /// <summary>
        /// Alternates between uploading either an empty file, or a file with data in it.
        /// </summary>
        /// <param name="diffMethod"></param>
        /// <param name="empty"></param>
        private void EmptyFileHandling(DiffMethods diffMethod, bool empty) 
        {
            Settings.DiffMethod = diffMethod;

            int passes = 3;
            int nonEmptyLength = 100;

            for (int i = 0; i < passes; i++) 
            {
                // set up an array of files to use as base
                List<DummyFile> inFiles = new List<DummyFile>() {
                    new DummyFile {
                        Data = DataHelper.GetRandomData(empty ? 0 : nonEmptyLength),
                        Path = Guid.NewGuid().ToString()
                    }
                };

                // create package from files array, zipped up
                this.PackageCreate.Create(new PackageCreateArguments
                {
                    Id = $"EmptyFileHandling{i}",
                    IsArchive = true,
                    Project = "some-project",
                    Files = FormFileHelper.FromStream(ArchiveHelper.ZipStreamFromFiles(inFiles), "archive.zip")
                });

                // retrieve this passes' package and manifest
                Stream outZip = this.IndexReader.GetPackageAsArchive("some-project", $"EmptyFileHandling{i}");
                IList<DummyFile> outFiles = ArchiveHelper.FilesFromZipStream(outZip).ToList();

                // confirm file data matches
                Assert.Equal(outFiles[0].Data.Length, inFiles[0].Data.Length);

                empty = !empty;
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
                Files = FormFileHelper.Multiple(new[] {
                    new DummyFile ( "some content", "folder1/file.txt" ),
                    new DummyFile ( "some content", "folder2/file.txt" )
                })
            };

            PackageCreateResult result = _packageService.Create(postArgs);
            
            Assert.False(result.Success);
            Assert.Equal(PackageCreateErrorTypes.InvalidFileCount, result.ErrorType);
        }

        /// <summary>
        /// Identical content is linked instead of patched
        /// </summary>
        [Fact]
        public void CreateLinked() 
        {
            // create first package
            _packageService.Create(new PackageCreateArguments
            {
                Id = "1",
                Project = "some-project",
                Files = FormFileHelper.Multiple(new [] {
                    new DummyFile("some content", "folder2/file.txt") 
                })
            });

            // create 2nd package with identical content to first
            _packageService.Create(new PackageCreateArguments
            {
                Id = "2",
                Project = "some-project",
                Files = FormFileHelper.Multiple(new[] {
                    new DummyFile( "some content", "folder2/file.txt")
                })
            });

            Manifest manifest = IndexReader.GetManifest("some-project", "2");
            Assert.Equal(ManifestItemTypes.Link, manifest.Files[0].Type);
        }

        /// <summary>
        /// Changed content is patched
        /// </summary>
        [Fact]
        public void CreatePatched()
        {
            // create first package
            _packageService.Create(new PackageCreateArguments
            {
                Id = "1",
                Project = "some-project",
                Files = FormFileHelper.Multiple(new [] {
                    new DummyFile ( "some content", "folder2/file.txt" )
                })
            });

            // create 2nd package with identical content to first
            _packageService.Create(new PackageCreateArguments
            {
                Id = "2",
                Project = "some-project",
                Files = FormFileHelper.Multiple(new [] {
                    new DummyFile ( "some content2", "folder2/file.txt" )
                })
            });

            Manifest manifest = IndexReader.GetManifest("some-project", "2");
            Assert.Equal(ManifestItemTypes.Patch, manifest.Files[0].Type);
        }

        #endregion
    }
}
