using System.IO;
using System.Text;
using Xunit;

namespace Tetrifact.Tests
{
    public class HashService : FileSystemBase
    {
        private readonly string _input = "test input";

        /// <summary>
        /// Known SHA256 output from the above input string
        /// </summary>
        private readonly string _expectedHash = "9dfe6f15d1ab73af898739394fd22fd72a03db01834582f24bb2e1c66c7aaeae";

        /// <summary>
        /// Hashing byte array works.
        /// </summary>
        [Fact]
        public void FromByteArray()
        {
            byte[] input = Encoding.ASCII.GetBytes(_input);
            Assert.Equal(_expectedHash, Core.HashService.FromByteArray(input));
        }

        /// <summary>
        /// Hashing a file on file system works.
        /// </summary>
        [Fact]
        public void FromFile()
        {
            Directory.CreateDirectory(this.Settings.TempPath);
            string path = Path.Join(this.Settings.TempPath, "hashFromFileTest.txt");
            File.WriteAllText(path, _input);
            Assert.Equal(_expectedHash, Core.HashService.FromFile(path));
        }

        /// <summary>
        /// Hashing a string works.
        /// </summary>
        [Fact]
        public void FromString()
        {
            Assert.Equal(_expectedHash, Core.HashService.FromString(_input));
        }

    }
}
