using System;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class PackageNameInUse : FileSystemBase
    {
        [Fact]
        public void InUse()
        {
            ISettings settings = TestContext.Get<ISettings>();

            string packageName = Guid.NewGuid().ToString();
            Directory.CreateDirectory(Path.Join(settings.PackagePath, packageName));

            Assert.True(this.IndexReader.PackageNameInUse(packageName));
        }

        [Fact]
        public void NotInUse()
        {
            string packageName = Guid.NewGuid().ToString();
            Assert.False(this.IndexReader.PackageNameInUse(packageName));
        }
    }
}
