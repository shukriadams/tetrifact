using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class GetIncomingFileNames : Base
    {
        [Fact]
        public void Get()
        {
            this.Workspace.AddIncomingFile(StreamsHelper.StreamFromString("content"), "path1/file1.txt");
            this.Workspace.AddIncomingFile(StreamsHelper.StreamFromString("content"), "path2/to/file2.txt");
            this.Workspace.AddIncomingFile(StreamsHelper.StreamFromString("content"), "file3.txt");

            string[] files = this.Workspace.GetIncomingFileNames().ToArray();
            Assert.Contains("path1/file1.txt", files);
            Assert.Contains("path2/to/file2.txt", files);
            Assert.Contains("file3.txt", files);
        }
    }
}
