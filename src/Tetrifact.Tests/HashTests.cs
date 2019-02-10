using System.IO;
using System.Text;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    public class HashTests : TestBase
    {
        string _input = "test input";
        string _expectedHash = "9dfe6f15d1ab73af898739394fd22fd72a03db01834582f24bb2e1c66c7aaeae";

        [Fact]
        public void FromByteArray()
        {
            byte[] input = Encoding.ASCII.GetBytes(_input);
            Assert.Equal(_expectedHash, HashService.FromByteArray(input));
        }

        [Fact]
        public void FromFile()
        {
            Directory.CreateDirectory(this.Settings.TempPath);
            string path = Path.Join(this.Settings.TempPath, "hashFromFileTest.txt");
            File.WriteAllText(path, _input);
            Assert.Equal(_expectedHash, HashService.FromFile(path));
        }

        [Fact]
        public void FromString()
        {
            Assert.Equal(_expectedHash, HashService.FromString(_input));
        }

    }
}
