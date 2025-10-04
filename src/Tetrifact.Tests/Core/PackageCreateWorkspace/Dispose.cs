using Moq;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class Dispose : Base
    {
        [Fact]
        public void Basic()
        {
            ISettings settings = TestContext.Get<ISettings>();

            string content = "a test file";
            Stream file = StreamsHelper.StreamFromString(content);
            this.PackageCreateWorkspace.AddIncomingFile(file, "test/file.txt");

            // Ensure that the workspace has content in it
            Assert.True(Directory.Exists(this.PackageCreateWorkspace.WorkspacePath));

            this.PackageCreateWorkspace.Dispose();

            // after flush,workspace should be empty
            Assert.False(Directory.Exists(this.PackageCreateWorkspace.WorkspacePath));

            // make sure we didn't do something stupid like nuke the entire temp folder, it's 
            // not like I've never done _that_ before
            Assert.True(Directory.Exists(settings.TempPath));
        }

        /// <summary>
        /// Coverage - ensure IO exception handling is hit
        /// </summary>
        [Fact]
        public void Error()
        {
            TestLogger<IPackageCreateWorkspace> workspaceLogger = new TestLogger<IPackageCreateWorkspace>();

            Mock<IFileSystem> fs = new Mock<IFileSystem>();
            fs.Setup(mq => mq.Directory.Exists(It.IsAny<string>()))
                .Callback(() => {
                    throw new IOException("some-error");
                });

            IPackageCreateWorkspace workspace = TestContext.Get<IPackageCreateWorkspace>("filesystem", fs.Object, "log", workspaceLogger);

            workspace.Dispose();
            Assert.True(workspaceLogger.ContainsFragment("Failed to delete temp folder"));
        }
    }
}
