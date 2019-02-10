using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageAsArchive : Base
    {
        [Fact]
        public async void GetBasic()
        {
            // create package, files folder and item location in one
            byte[] content = Encoding.ASCII.GetBytes("some content");
            string path = "path/to/file";
            string package = "somepackage";
            
            Core.IWorkspace workspace = new Core.Workspace(this.Settings);
            await workspace.AddIncomingFileAsync(Core.StreamsHelper.StreamFromString("some content"), path);
            workspace.WriteFile(path, "somehash", package);
            workspace.WriteManifest(package, "somehash2");

            Stream testContent = await this.IndexReader.GetPackageAsArchiveAsync(package);
            Dictionary<string, byte[]> items = Core.StreamsHelper.ArchiveStreamToCollection(testContent);

            Assert.Single(items);
            Assert.Equal(content, items[path]);
        }
    }
}
