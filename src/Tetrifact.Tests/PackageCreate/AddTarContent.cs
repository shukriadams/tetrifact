using System.IO;
using System.IO.Compression;
using Tetrifact.Core;
using Xunit;
using SharpCompress.Common;
using SharpCompress.Writers.Tar;

namespace Tetrifact.Tests.Workspace
{
    public class AddTarContent
    {
        /*
        [Fact]
        public void AddContent()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (TarWriter writer = new TarWriter(memoryStream, new TarWriterOptions(CompressionType.None, true)))             {
                    //writer.Write("path/file.txt", StreamsHelper.StreamFromString("content"), null);
                }
                base.Workspace.AddZipContent(memoryStream);
                string filePath = Path.Combine(base.Workspace.WorkspacePath, "incoming", "path", "file.txt");
                string readContent = File.ReadAllText(filePath);
                Assert.Equal("content", readContent);
            }
        }
        */
    }
}