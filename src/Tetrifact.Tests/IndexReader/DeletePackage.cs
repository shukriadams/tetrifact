using System.IO;
using System.Text;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class DeletePackage : FileSystemBase
    {
        [Fact]
        public void BasicDelete()
        {
            TestPackage testPackage = base.CreatePackage();

            this.IndexReader.DeletePackage(testPackage.Name);

            Assert.False(File.Exists(Path.Combine(this.Settings.PackagePath, "manifest.json" )));
        }
    
        /// <summary>
        /// Same as BasicDelete(), but handles archive deleting too
        /// </summary>
        [Fact]
        public void DeleteWithArchive()
        {
            TestPackage testPackage = base.CreatePackage();

            // mock archive
            string archivePath = base.IndexReader.GetPackageArchivePath(testPackage.Name);
            File.WriteAllText(archivePath, string.Empty);

            this.IndexReader.DeletePackage(testPackage.Name);

            Assert.False(File.Exists(archivePath));
        }

        // [Fact] disabled because this fails on travis
        public void DeleteWithLockedArchive()
        {
            TestPackage testPackage = base.CreatePackage();

            // mock archive
            string archivePath = base.IndexReader.GetPackageArchivePath(testPackage.Name);
            File.WriteAllText(archivePath, string.Empty);

            // force create dummy zip file in archive folder
            File.WriteAllText(archivePath, "dummy content");

            // open stream in write mode to lock it, then attempt to purge archives
            using (FileStream fs = File.OpenWrite(archivePath))
            {
                // force write something to stream to ensure it locks
                fs.Write(Encoding.ASCII.GetBytes("random"));

                this.IndexReader.DeletePackage(testPackage.Name);

                Assert.Single(base.Logger.LogEntries);
                Assert.Contains("Failed to purge archive", base.Logger.LogEntries[0]);
            }
        }

        [Fact]
        public void InvalidPackage()
        {
            string packageId = "invalidId";
            PackageNotFoundException ex = Assert.Throws<PackageNotFoundException>(()=> this.IndexReader.DeletePackage(packageId));
            Assert.Equal(ex.PackageId, packageId);
        }

    }
}
