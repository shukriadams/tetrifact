using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    public class PackageCorruptExceptionTests
    {
        /// <summary>
        /// Coverage tests
        /// </summary>
        [Fact]
        public void Constructors()
        {
            new PackageCorruptException("");
            new PackageCorruptException("", null);
        }
    }
}
