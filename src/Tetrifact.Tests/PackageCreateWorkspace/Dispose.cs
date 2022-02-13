using Moq;
using Ninject;
using Ninject.Parameters;
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
            Assert.True(Directory.Exists(base.Settings.TempPath));
        }

        /// <summary>
        /// Coverage - ensure IO exception handling is hit
        /// </summary>
        [Fact]
        public void Error()
        {
            Mock<IFileSystem> fs = new Mock<IFileSystem>();
            fs.Setup(mq => mq.Directory.Exists(It.IsAny<string>()))
                .Callback(() => {
                    throw new IOException("some-error");
                });

            TestLogger<IPackageCreateWorkspace> log = new TestLogger<IPackageCreateWorkspace>();

            IPackageCreateWorkspace workspace = this.Kernel.Get<IPackageCreateWorkspace>(new ConstructorArgument[] { new ConstructorArgument("filesystem", fs.Object), new ConstructorArgument("log", log) } );
            workspace.Dispose();
            Assert.True(log.ContainsFragment("Failed to delete temp folder"));
        }
    }
}
