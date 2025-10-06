using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetExpectedManifest : TestBase
    {
        /// <summary>
        /// Coverage
        /// </summary>
        [Fact]
        public void InvalidManifest()
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();
            Assert.Throws<PackageNotFoundException>(()=>{ indexReader.GetExpectedManifest("invalid id"); });
        }
    }
}
