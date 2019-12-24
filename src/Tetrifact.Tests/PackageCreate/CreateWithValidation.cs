using System.Collections.Generic;
using Xunit;
using Ninject;
using Tetrifact.Core;
using System.Linq;
using System.IO;
using System;
using Tetrifact.Dev;
using System.Text;

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

        [Fact]
        public void AddZipStackedBsDiff() 
        {
            AddZipStacked(DiffMethods.BsDiff);
        }

        [Fact]
        public void AddZipStackedVcDiff()
        {
            AddZipStacked(DiffMethods.VcDiff);
        }

        /// <summary>
        /// Creates mulitple packages with same files but changing file content. This tests patching logic.
        /// </summary>
        private void AddZipStacked(DiffMethods diffMethod) 
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

        [Fact]
        public void CompareDiffMethods() 
        {

            StringBuilder times = new StringBuilder();
            foreach (DiffMethods diffMethod in Enum.GetValues(typeof(DiffMethods)))
            {
                Settings.DiffMethod = diffMethod;
                IList<DummyFile> inFiles = new List<DummyFile>() {
                    new DummyFile
                    {
                        Data = DataHelper.GetRandomData(1024),
                        Path = Guid.NewGuid().ToString()
                    }
                };

                // need project per diffMethod to keep their comparitive diffing separate
                IProjectService projectService = new Core.ProjectService(Settings, new TestLogger<IProjectService>());
                string projectName = diffMethod.ToString();
                projectService.Create(projectName);

                int passes = 3;
                for (int i = 0; i < passes; i++)
                {
                    string packageName = $"AddZipLinked{i}{diffMethod}";
                    
                    DateTime start = DateTime.Now;

                    // create package from files array, zipped up
                    this.PackageCreate.CreateWithValidation(new PackageCreateArguments
                    {
                        Id = packageName,
                        IsArchive = true,
                        Project = projectName,
                        Files = FormFileHelper.FromStream(ArchiveHelper.ZipStreamFromFiles(inFiles), "archive.zip")
                    });

                    times.AppendLine($"{diffMethod} add {i}, took {(DateTime.Now - start).TotalMilliseconds} ms");
                    start = DateTime.Now;

                    // retrieve this passes' package and manifest
                    Stream outZip = this.IndexReader.GetPackageAsArchive(projectName, packageName);
                    times.AppendLine($"{diffMethod} retrieve {i}, took {(DateTime.Now - start).TotalMilliseconds} ms");

                    Manifest manifest = this.IndexReader.GetManifest(projectName, packageName);
                    times.AppendLine($"{diffMethod} pass {i}, {manifest.Compressed}% saved.");
                    outZip.Dispose();

                    // update files for next pass
                    foreach (DummyFile file in inFiles)
                        file.Data = DataHelper.GetRandomData(104).Concat(file.Data).ToArray();
                }
            }

            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "diff-performance.txt"), times.ToString());
        }
    }
}
