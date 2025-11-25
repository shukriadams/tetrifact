using System;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class PackageNameInUse
    {
        private TestContext _testContext = new TestContext();
        
        [Fact]
        public void InUse()
        {
            ISettings settings = _testContext.Get<ISettings>();
            IIndexReadService indexReader = _testContext.Get<IIndexReadService>();

            string packageName = Guid.NewGuid().ToString();
            Directory.CreateDirectory(Path.Join(settings.PackagePath, packageName));

            Assert.True(indexReader.PackageNameInUse(packageName));
        }

        [Fact]
        public void NotInUse()
        {
            IIndexReadService indexReader = _testContext.Get<IIndexReadService>();
            string packageName = Guid.NewGuid().ToString();
            Assert.False(indexReader.PackageNameInUse(packageName));
        }
    }
}
