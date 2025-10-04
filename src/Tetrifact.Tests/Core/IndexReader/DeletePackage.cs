using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class DeletePackage : FileSystemBase
    {
        /// <summary>
        /// walks all package delete logic, successful output detected as coverage
        /// </summary>
        [Fact]
        public void HappyPath()
        {
            Mock<IFileSystem> fileSystem = new Mock<IFileSystem>();
            
            // force files to exist to enable deletes
            fileSystem
                .Setup(r => r.File.Exists(It.IsAny<string>()))
                .Returns(true);

            // force dirs to exist to enable deletes
            fileSystem
                .Setup(r => r.Directory.Exists(It.IsAny<string>()))
                .Returns(true);

            // return some files to trigger tag file deletes
            fileSystem
                .Setup(r => r.Directory.GetFiles(It.IsAny<string>()))
                .Returns(new string[] { "a tag file", "another tag file" });

            Mock<IndexReadService> indexReader = MoqHelper.CreateMockWithDependencies<IndexReadService>( new object[] { fileSystem });
            
            // return a manifest to avoid non-found exception. Add an item to trigger item handling logic
            indexReader
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Returns(new Manifest() { Files = new List<ManifestItem> { new ManifestItem { Path = "any path", Hash = "any hash" } }});

            // delete, no return value, coverage implies success
            indexReader.Object.DeletePackage("any package id");
        }

        
        [Fact]
        public void DeleteDisabled()
        {
            ISettings settings = TestContext.Get<ISettings>();
            settings.PackageDeleteEnabled = false;
            TestPackage testPackage = PackageHelper.CreateRandomPackage();
            OperationNowAllowedException ex = Assert.Throws<OperationNowAllowedException>(() => this.IndexReader.DeletePackage(testPackage.Id));
            Assert.True(File.Exists(Path.Combine(settings.PackagePath, testPackage.Id, "manifest.json")));
        }

        /// <summary>
        /// Same as BasicDelete(), but handles archive deleting too
        /// </summary>
        [Fact]
        public void DeleteWithArchive()
        {
            TestPackage testPackage = PackageHelper.CreateRandomPackage();
            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            // mock archive
            PackageHelper.FakeArchiveOnDisk(testPackage);

            this.IndexReader.DeletePackage(testPackage.Id);

            string archivePath = archiveService.GetPackageArchivePath(testPackage.Id);
            Assert.False(File.Exists(archivePath));
        }
       

        [Fact]
        public void InvalidPackage()
        {
            string packageId = "invalidId";
            PackageNotFoundException ex = Assert.Throws<PackageNotFoundException>(()=> this.IndexReader.DeletePackage(packageId));
            Assert.Equal(ex.PackageId, packageId);
        }


        /// <summary>
        /// An IOException on archive delete should be trapped, when the archive is still in use.
        /// </summary>
        [Fact]
        public void LockedArchive()
        {
            TestPackage testPackage = PackageHelper.CreateRandomPackage();
            IArchiveService archiveService = TestContext.Get<IArchiveService>();

            // mock its archive
            string archivePath = archiveService.GetPackageArchivePath(testPackage.Id);
            File.WriteAllText(archivePath, string.Empty);

            // lock the archive by opening a read stream on it
            using(new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // no need to assert, this test is for coverage, and all we want is no exception to be thrown here
                this.IndexReader.DeletePackage(testPackage.Id);
            }
        }

        /// <summary>
        /// An IOException on tag delete should be trapped, when the tag is still in use.
        /// </summary>
        [Fact]
        public void LockedTag()
        {
            TestPackage testPackage = PackageHelper.CreateRandomPackage();
            ISettings settings = TestContext.Get<ISettings>();
            ITagsService tagsService = TestContext.Get<ITagsService>();
            
            tagsService.AddTag(testPackage.Id, "mytag");

            string[] tagDirectories = Directory.GetDirectories(Path.Join(settings.TagsPath));
            Assert.Single(tagDirectories); // should be 1 only

            string[] tagSubscribers = Directory.GetFiles(tagDirectories.First());
            Assert.Single(tagSubscribers); // should be 1 only

            // lock the tag by opening a read stream on it
            using (new FileStream(tagSubscribers.First(), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // no need to assert, this test is for coverage, and all we want is no exception to be thrown here
                this.IndexReader.DeletePackage(testPackage.Id);
            }
        }
    }
}
