using System.IO;
using System.IO.Compression;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class AddArchiveContent : Base
    {
        
        [Fact]
        public void Add()
        {
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

                base.PackageCreateWorkspace.AddArchiveContent(memoryStream);
                string filePath = Path.Combine(base.PackageCreateWorkspace.WorkspacePath, "incoming", "path", "to", "file.txt");
                string readContent = File.ReadAllText(filePath);
                Assert.Equal("content", readContent);
            }
        }
    }
}
