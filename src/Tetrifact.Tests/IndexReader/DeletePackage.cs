using System.IO;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class DeletePackage : FileSystemBase
    {
        [Fact]
        public void BasicDelete()
        {
            // create package, files folder and item location in one
            string path = "path/to/file";
            string package = "somepackage";

            Core.IWorkspace workspace = new Core.Workspace(this.Settings);
            workspace.AddIncomingFile(Core.StreamsHelper.StreamFromString("some content"), path);
            workspace.WriteFile(path, "somehash", package);
            workspace.WriteManifest(package, "somehash2");

            this.IndexReader.DeletePackage(package);

            Assert.False(File.Exists(Path.Combine(this.Settings.PackagePath, "manifest.json" )));
        }
    }
}
