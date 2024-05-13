using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    public class PageableDataTests : TestBase
    {
        /// <summary>
        /// Coverage compliance tests, ensures that modulus condition in PageableData ctor is reached.
        /// </summary>
        [Fact]
        public void Modulus()
        {
            string[] items = new string[]{"1", "2", "3"};
            new PageableData<string>(items, 0, 2, 3);
        }
    }
}