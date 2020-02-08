using System.IO;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.HashService
{
    [Collection("Tests")]
    public class FromFile : Base
    {
        /// <summary>
        /// Hashing a file on file system works.
        /// </summary>
        [Fact]
        public void Basic()
        {
            Directory.CreateDirectory(Settings.TempPath);
            string path = Path.Join(Settings.TempPath, "hashFromFileTest.txt");
            File.WriteAllText(path, _input);
            Assert.Equal(_expectedHash, Core.HashService.FromFile(path));
        }
    }
}
