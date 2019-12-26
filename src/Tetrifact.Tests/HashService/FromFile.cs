using System.IO;
using Xunit;

namespace Tetrifact.Tests.HashService
{
    public class FromFile : Base
    {
        /// <summary>
        /// Hashing a file on file system works.
        /// </summary>
        [Fact]
        public void Basic()
        {
            Directory.CreateDirectory(this.Settings.TempPath);
            string path = Path.Join(this.Settings.TempPath, "hashFromFileTest.txt");
            File.WriteAllText(path, _input);
            Assert.Equal(_expectedHash, Core.HashService.FromFile(path));
        }
    }
}
