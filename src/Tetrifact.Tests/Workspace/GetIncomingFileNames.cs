using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class GetIncomingFileNames : Base
    {
        [Fact]
        async public void Get()
        {
            await this.Workspace.AddIncomingFileAsync(StreamsHelper.StreamFromString("content"), "path1/file1.txt");
            await this.Workspace.AddIncomingFileAsync(StreamsHelper.StreamFromString("content"), "path2/to/file2.txt");
            await this.Workspace.AddIncomingFileAsync(StreamsHelper.StreamFromString("content"), "file3.txt");

            string[] files = this.Workspace.GetIncomingFileNames().ToArray();
            Assert.Contains(Path.Join("path1", "file1.txt"), files);
            Assert.Contains(Path.Join("path2", "to", "file2.txt"), files);
            Assert.Contains("file3.txt", files);
        }
    }
}
