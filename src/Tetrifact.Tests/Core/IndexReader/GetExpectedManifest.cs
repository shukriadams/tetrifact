using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetExpectedManifest : FileSystemBase
    {
        /// <summary>
        /// Coverage
        /// </summary>
        [Fact]
        public void InvalidManifest()
        { 
            Assert.Throws<PackageNotFoundException>(()=>{ base.IndexReader.GetExpectedManifest("invalid id"); });
        }
    }
}
