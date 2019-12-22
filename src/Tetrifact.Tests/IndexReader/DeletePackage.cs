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
            DummyPackage testPackage = base.CreatePackage();

            this.PackageDeleter.Delete("some-project", testPackage.Id);

            Assert.False(File.Exists(Path.Combine(this.Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "manifest.json" )));
        }
    
        // [Fact] disabled because this fails on travis
        public void DeleteWithLockedArchive()
        {
            DummyPackage testPackage = base.CreatePackage();

            // mock archive
            string archivePath = base.IndexReader.GetPackageArchivePath("some-project", testPackage.Id);
            File.WriteAllText(archivePath, string.Empty);

            // force create dummy zip file in archive folder
            File.WriteAllText(archivePath, "dummy content");

            // open stream in write mode to lock it, then attempt to purge archives
            using (FileStream fs = File.OpenWrite(archivePath))
            {
                // force write something to stream to ensure it locks
                fs.Write(Encoding.ASCII.GetBytes("random"));

                this.PackageDeleter.Delete("some-project", testPackage.Id);

                Assert.Single(base.Logger.LogEntries);
                Assert.Contains("Failed to purge archive", base.Logger.LogEntries[0]);
            }
        }

        [Fact]
        public void InvalidProject()
        {
            string project = "not-a-valid-project-id";
            ProjectNotFoundException ex = Assert.Throws<ProjectNotFoundException>(() => this.PackageDeleter.Delete(project, "invalidId"));
            Assert.Equal(project, ex.Project);
        }

        [Fact]
        public void InvalidPackage()
        {
            string packageId = "invalidId";
            PackageNotFoundException ex = Assert.Throws<PackageNotFoundException>(()=> this.PackageDeleter.Delete("some-project", packageId));
            Assert.Equal(ex.PackageId, packageId);
        }

    }
}
