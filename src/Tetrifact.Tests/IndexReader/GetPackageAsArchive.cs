using System.Collections.Generic;
using System.IO;
using System.Text;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageAsArchive : FileSystemBase
    {
        [Fact]
        public void GetBasic()
        {
            // create package, files folder and item location in one
            byte[] content = Encoding.ASCII.GetBytes("some content");
            string path = "path/to/file";
            string package = "somepackage";

            Core.IWorkspace workspace = new Core.Workspace(this.Settings);
            workspace.AddIncomingFile(Core.StreamsHelper.StreamFromString("some content"), path);
            workspace.WriteFile(path, "somehash", package);
            workspace.WriteManifest(package, "somehash2");

            Stream testContent = this.IndexReader.GetPackageAsArchive(package);
            Dictionary<string, byte[]> items = Core.StreamsHelper.ArchiveStreamToCollection(testContent);

            Assert.Single(items);
            Assert.Equal(content, items[path]);
        }

        [Fact]
        public void GetNonExistent()
        {
            try
            {
                this.IndexReader.GetPackageAsArchive("invalid id");
                Assert.True(false);
            }
            catch (PackageNotFoundException ex)
            {
                Assert.Equal("invalid id", ex.PackageId);
            }
        }

    }
}
