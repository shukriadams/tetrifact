using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetExpectedManifest
    {
        private TestContext _testContext = new TestContext();
        
        /// <summary>
        /// Coverage
        /// </summary>
        [Fact]
        public void InvalidManifest()
        {
            IIndexReadService indexReader = _testContext.Get<IIndexReadService>();
            Assert.Throws<PackageNotFoundException>(()=>{ indexReader.GetExpectedManifest("invalid id"); });
        }
    }
}
