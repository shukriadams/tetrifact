using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class Dispose : Base
    {
        [Fact]
        public void Basic()
        {
            string content = "a test file";
            Stream file = StreamsHelper.StreamFromString(content);
            this.Workspace.AddFile(file, "test/file.txt");

            // Ensure that the workspace has content in it
            Assert.True(Directory.Exists(this.Workspace.WorkspacePath));

            this.Workspace.Dispose();

            // after flush,workspace should be empty
            Assert.False(Directory.Exists(this.Workspace.WorkspacePath));

            // make sure we didn't do something stupid like nuke the entire temp folder, it's 
            // not like I've never done _that_ before
            Assert.True(Directory.Exists(base.Settings.TempPath));
        }
    }
}
