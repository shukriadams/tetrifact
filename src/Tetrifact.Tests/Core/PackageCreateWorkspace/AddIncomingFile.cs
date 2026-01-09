using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class AddIncomingFile
    {
        private readonly TestContext _testContext = new TestContext();

        [Fact]
        public void Add()
        {
            IPackageCreateWorkspace packageCreateWorkspace = _testContext.Instantiate<IPackageCreateWorkspace>();

            string content = "a test file";
            Stream file = StreamsHelper.StreamFromString(content);
            packageCreateWorkspace.AddIncomingFile(file, "test/file.txt");

            string testContent = File.ReadAllText(Path.Join(packageCreateWorkspace.WorkspacePath, "incoming/test/file.txt"));
            Assert.Equal(testContent, content);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void EmptyStream()
        {
            // returns false if attempting to send empty stream 
            IPackageCreateWorkspace workspace = _testContext.Instantiate<IPackageCreateWorkspace>();
            bool result = workspace.AddIncomingFile(StreamsHelper.StreamFromString(string.Empty), string.Empty);
            Assert.False(result);
        }
    }
}
