using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class AddIncomingFile : Base
    {
        private readonly TestContext _testContext = new TestContext();

        [Fact]
        public void Add()
        {
            string content = "a test file";
            Stream file = StreamsHelper.StreamFromString(content);
            this.PackageCreateWorkspace.AddIncomingFile(file, "test/file.txt");

            string testContent = File.ReadAllText(Path.Join(this.PackageCreateWorkspace.WorkspacePath, "incoming/test/file.txt"));
            Assert.Equal(testContent, content);
        }

        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void EmptyStream()
        {
            // returns false if attempting to send empty stream 
            IPackageCreateWorkspace workspace = _testContext.Get<IPackageCreateWorkspace>();
            bool result = workspace.AddIncomingFile(StreamsHelper.StreamFromString(string.Empty), string.Empty);
            Assert.False(result);
        }
    }
}
