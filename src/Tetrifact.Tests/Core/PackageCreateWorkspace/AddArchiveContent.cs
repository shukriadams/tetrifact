using System.IO;
using System.IO.Compression;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class AddArchiveContent
    {
        private readonly TestContext _testContext = new TestContext();

        [Fact]
        public void Add()
        {
            IPackageCreateWorkspace packageCreateWorkspace = _testContext.Instantiate<IPackageCreateWorkspace>();
            packageCreateWorkspace.Initialize();  

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry demoFile = archive.CreateEntry("path/to/file.txt");
                    using (Stream entryStream = demoFile.Open())
                    using (StreamWriter streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write("content");
                    }
                }

                packageCreateWorkspace.AddArchiveContent(memoryStream);
                string filePath = Path.Combine(packageCreateWorkspace.WorkspacePath, "incoming", "path", "to", "file.txt");
                string readContent = File.ReadAllText(filePath);
                Assert.Equal("content", readContent);
            }
        }
    }
}
