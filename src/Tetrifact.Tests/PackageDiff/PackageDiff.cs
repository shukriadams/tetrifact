using System;
using System.Collections.Generic;
using System.Text;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageDiff
{
    public class PackageDiff : FileSystemBase
    {
        IPackageDiffService PackageDiffService;
        TestLogger<IPackageDiffService> Logger;

        public PackageDiff()
        { 
            this.Logger = new TestLogger<IPackageDiffService>();
            this.PackageDiffService = new PackageDiffService(this.Settings, this.FileSystem, this.IndexReader, this.Logger);
        }

        [Fact]
        public void HappyPath()
        { 
            TestPackage upstreamPackage = PackageHelper.CreatePackage(Settings);
            TestPackage downstreaPackage = PackageHelper.CreatePackage(Settings);

            // force delete cached diff 
            CleanupHelper.ClearDirectory(Settings.PackageDiffsPath);


            Core.PackageDiff diff = this.PackageDiffService.GetDifference(upstreamPackage.Id, downstreaPackage.Id);
            // get twice to hit cached version too
            diff = this.PackageDiffService.GetDifference(upstreamPackage.Id, downstreaPackage.Id);
            Assert.Equal(downstreaPackage.Id, diff.DownstreamPackageId);
            Assert.Equal(upstreamPackage.Id, diff.UpstreamPackageId);
        }
    }
}
