using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class AddIncomingFile : Base
    {
        [Fact]
        public void Add()
        {
            string content = "a test file";
            Stream file = StreamsHelper.StreamFromString(content);
            this.Workspace.AddFile(file, "test/file.txt");

            string testContent = File.ReadAllText(Path.Join(this.Workspace.WorkspacePath, "incoming/test/file.txt"));
            Assert.Equal(testContent, content);
        }
    }
}
