using System.IO;
using Tetrifact.Core;
using Tetrifact.Dev;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class Delete : FileSystemBase
    {
        [Fact]
        public void BasicDelete()
        {
            DummyPackage testPackage = base.CreatePackage();

            this.PackageDeleter.Delete("some-project", testPackage.Id);

            Assert.False(File.Exists(Path.Combine(this.Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "manifest.json" )));
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
