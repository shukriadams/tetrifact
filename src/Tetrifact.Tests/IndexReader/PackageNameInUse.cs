using System;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    [Collection("Tests")]
    public class PackageNameInUse : FileSystemBase
    {
        [Fact]
        public void InUse()
        {
            string packageName = Guid.NewGuid().ToString();
            this.CreatePackage(packageName);

            Assert.True(this.IndexReader.PackageNameInUse("some-project", packageName));
        }

        [Fact]
        public void NotInUse()
        {
            string packageName = Guid.NewGuid().ToString();
            Assert.False(this.IndexReader.PackageNameInUse("some-project", packageName));
        }
    }
}
