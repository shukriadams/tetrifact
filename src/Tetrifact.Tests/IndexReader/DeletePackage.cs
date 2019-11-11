using Microsoft.Extensions.Logging;
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

            this.IndexReader.DeletePackage("some-project", testPackage.Name);

            Assert.False(File.Exists(Path.Combine(this.Settings.ProjectsPath, "some-project", Constants.PackagesFragment, "manifest.json" )));
        }
    
        // [Fact] disabled because this fails on travis
        public void DeleteWithLockedArchive()
        {
            TestPackage testPackage = base.CreatePackage();

            // mock archive
            string archivePath = base.IndexReader.GetPackageArchivePath("some-project", testPackage.Name);
            File.WriteAllText(archivePath, string.Empty);

            // force create dummy zip file in archive folder
            File.WriteAllText(archivePath, "dummy content");

            // open stream in write mode to lock it, then attempt to purge archives
            using (FileStream fs = File.OpenWrite(archivePath))
            {
                // force write something to stream to ensure it locks
                fs.Write(Encoding.ASCII.GetBytes("random"));

                this.IndexReader.DeletePackage("some-project", testPackage.Name);

                Assert.Single(base.Logger.LogEntries);
                Assert.Contains("Failed to purge archive", base.Logger.LogEntries[0]);
            }
        }

        [Fact]
        public void InvalidProject()
        {
            ProjectNotFoundException ex = Assert.Throws<ProjectNotFoundException>(() => this.IndexReader.DeletePackage("some-project", "invalidId"));
            Assert.Equal("some-project", ex.Project);
        }

        [Fact]
        public void InvalidPackage()
        {
            this.InitProject();
            string packageId = "invalidId";
            PackageNotFoundException ex = Assert.Throws<PackageNotFoundException>(()=> this.IndexReader.DeletePackage("some-project", packageId));
            Assert.Equal(ex.PackageId, packageId);
        }

    }
}
