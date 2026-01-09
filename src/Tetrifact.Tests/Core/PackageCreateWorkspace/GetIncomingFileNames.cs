using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class GetIncomingFileNames
    {
        private readonly TestContext _testContext = new TestContext();

        [Fact]
        public void Get()
        {
            IPackageCreateWorkspace packageCreateWorkspace = _testContext.Instantiate<IPackageCreateWorkspace>();
            packageCreateWorkspace.Initialize();  

            packageCreateWorkspace.AddIncomingFile(StreamsHelper.StreamFromString("content"), "path1/file1.txt");
            packageCreateWorkspace.AddIncomingFile(StreamsHelper.StreamFromString("content"), "path2/to/file2.txt");
            packageCreateWorkspace.AddIncomingFile(StreamsHelper.StreamFromString("content"), "file3.txt");

            string[] files = packageCreateWorkspace.GetIncomingFileNames().ToArray();
            Assert.Contains("path1/file1.txt", files);
            Assert.Contains("path2/to/file2.txt", files);
            Assert.Contains("file3.txt", files);
        }
    }
}
