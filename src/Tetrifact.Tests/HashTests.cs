using System.IO;
using System.Text;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    public class HashTests
    {
        private TestContext _testContext = new TestContext();

        private readonly string _input = "test input";
        
        private readonly string _expectedHash = "9dfe6f15d1ab73af898739394fd22fd72a03db01834582f24bb2e1c66c7aaeae";

        [Fact]
        public void FromByteArray()
        {
            IHashService hashService = new HashService();

            byte[] input = Encoding.ASCII.GetBytes(_input);
            
            Assert.Equal(_expectedHash, hashService.FromByteArray(input));
        }

        [Fact]
        public void FromFile()
        {
            IHashService hashService = new HashService();
            ISettings settings = _testContext.Get<ISettings>();
            Directory.CreateDirectory(settings.TempPath);
            string path = Path.Join(settings.TempPath, "hashFromFileTest.txt");
            File.WriteAllText(path, _input);
            Assert.Equal(_expectedHash, hashService.FromFile(path).Hash);
        }

        [Fact]
        public void FromString()
        {
            IHashService hashService = new HashService();
            Assert.Equal(_expectedHash, hashService.FromString(_input));
        }

    }
}
