using System;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class PackageNameInUse : TestBase
    {
        [Fact]
        public void InUse()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            string packageName = Guid.NewGuid().ToString();
            Directory.CreateDirectory(Path.Join(settings.PackagePath, packageName));

            Assert.True(indexReader.PackageNameInUse(packageName));
        }

        [Fact]
        public void NotInUse()
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();
            string packageName = Guid.NewGuid().ToString();
            Assert.False(indexReader.PackageNameInUse(packageName));
        }
    }
}
