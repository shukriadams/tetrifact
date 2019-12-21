using System.IO;
using System.IO.Compression;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class AddZipContent : Base
    {
        [Fact]
        public void AddContent()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = archive.CreateEntry("path/file.txt");
                    using (var entryStream = demoFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write("content");
                    }
                }

                base.Workspace.AddArchive(memoryStream, Core.ArchiveTypes.zip);
                string filePath = Path.Combine(base.Workspace.WorkspacePath, "incoming", "path", "file.txt");
                string readContent = File.ReadAllText(filePath);
                Assert.Equal("content", readContent);
            }
        }
    }
}
