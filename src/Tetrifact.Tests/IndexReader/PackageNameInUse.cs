using System;
using System.IO;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.IndexReader
{
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
