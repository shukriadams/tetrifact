using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    [Collection("Tests")]
    public class Basic
    {
        /// <summary>
        /// Coverage compliance. Ensures modulus condition in PageableData ctor is reached by adding odd number of page items.
        /// </summary>
        [Fact]
        public void Modulus()
        {
            string[] items = new string[]{"1", "2", "3"};
            new PageableData<string>(items, 0, 2, 3);
        }
    }
}