using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class DeletePackage : FileSystemBase
    {

        [Fact]
        public void HappyPath()
        {
            TestPackage testPackage = PackageHelper.CreateNewPackageFile(this.Settings);
            Assert.True(File.Exists(Path.Combine(this.Settings.PackagePath, testPackage.Id, "manifest.json")));

            this.IndexReader.DeletePackage(testPackage.Id);

            Assert.False(File.Exists(Path.Combine(this.Settings.PackagePath, "manifest.json" )));
        }

        
        [Fact]
        public void DeleteDisabled()
        {
            this.Settings.AllowPackageDelete = false;
            TestPackage testPackage = PackageHelper.CreateNewPackageFile(this.Settings);
            OperationNowAllowedException ex = Assert.Throws<OperationNowAllowedException>(() => this.IndexReader.DeletePackage(testPackage.Id));
            Assert.True(File.Exists(Path.Combine(this.Settings.PackagePath, testPackage.Id, "manifest.json")));
        }

        /// <summary>
        /// Same as BasicDelete(), but handles archive deleting too
        /// </summary>
        [Fact]
        public void DeleteWithArchive()
        {
            TestPackage testPackage = PackageHelper.CreateNewPackageFile(this.Settings);

            // mock archive
            string archivePath = base.ArchiveService.GetPackageArchivePath(testPackage.Id);
            File.WriteAllText(archivePath, string.Empty);

            this.IndexReader.DeletePackage(testPackage.Id);

            Assert.False(File.Exists(archivePath));
        }

        
        [Fact (Skip = "fails consistently on travis")]  
        public void DeleteWithLockedArchive()
        {
            TestPackage testPackage = PackageHelper.CreateNewPackageFile(this.Settings);

            // mock archive
            string archivePath = base.ArchiveService.GetPackageArchivePath(testPackage.Id);
            File.WriteAllText(archivePath, string.Empty);

            // force create dummy zip file in archive folder
            File.WriteAllText(archivePath, "dummy content");

            // open stream in write mode to lock it, then attempt to purge archives
            using (FileStream lockStream = File.OpenWrite(archivePath))
            {
                this.IndexReader.DeletePackage(testPackage.Id);

                Assert.Single(base.IndexReaderLogger.LogEntries);
                Assert.Contains("Failed to purge archive", base.IndexReaderLogger.LogEntries[0]);
            }
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
            TestPackage testPackage = PackageHelper.CreateNewPackageFile(this.Settings);

            // mock its archive
            string archivePath = base.ArchiveService.GetPackageArchivePath(testPackage.Id);
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
            TestPackage testPackage = PackageHelper.CreateNewPackageFile(this.Settings);

            IMemoryCache _memoryCache = MemoryCacheHelper.GetInstance();
            PackageListCache PackageListCache = new PackageListCache(_memoryCache);
            ITagsService tagsService = new Core.TagsService(this.Settings, new FileSystem(), new TestLogger<ITagsService>(), PackageListCache);
            
            tagsService.AddTag(testPackage.Id, "mytag");

            string[] tagDirectories = Directory.GetDirectories(Path.Join(this.Settings.TagsPath));
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
