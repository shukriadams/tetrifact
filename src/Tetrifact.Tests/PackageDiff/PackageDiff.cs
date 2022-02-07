using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageDiff
{
    public class PackageDiff : FileSystemBase
    {
        IPackageDiffService PackageDiffService;
        new TestLogger<IPackageDiffService> Logger;

        public PackageDiff()
        { 
            this.Settings.WorkerThreadCount = 1;
            this.Logger = new TestLogger<IPackageDiffService>();
            this.PackageDiffService = new PackageDiffService(this.Settings, this.FileSystem, this.IndexReader, this.Logger);
        }

        [Fact]
        public void HappyPath_SingleThread()
        {
            this.Settings.WorkerThreadCount = 1;
            this.PackageDiffService = new PackageDiffService(this.Settings, this.FileSystem, this.IndexReader, this.Logger);

            string upstreamPackageId = PackageHelper.CreatePackage(Settings, new string[]{ "same content", "packege 1 content", "same content" } );
            string downstreamPackageId = PackageHelper.CreatePackage(Settings, new string[] { "same content", "packege 2 content", "same content" });

            // get diff
            Core.PackageDiff diff = this.PackageDiffService.GetDifference(upstreamPackageId, downstreamPackageId);
            
            // get diff again, to hit cached version too. This is for coverage.
            diff = this.PackageDiffService.GetDifference(upstreamPackageId, downstreamPackageId);

            Assert.Equal(downstreamPackageId, diff.DownstreamPackageId);
            Assert.Equal(upstreamPackageId, diff.UpstreamPackageId);
            Assert.Single(diff.Difference);
            Assert.Equal(2, diff.Common.Count);
        }

        [Fact]
        public void HappyPath_MultiThread()
        {
            this.Settings.WorkerThreadCount = 2;
            this.PackageDiffService = new PackageDiffService(this.Settings, this.FileSystem, this.IndexReader, this.Logger);

            string upstreamPackageId = PackageHelper.CreatePackage(Settings, new string[] { "same content", "packege 1 content", "same content" });
            string downstreamPackageId = PackageHelper.CreatePackage(Settings, new string[] { "same content", "packege 2 content", "same content" });

            // get diff
            Core.PackageDiff diff = this.PackageDiffService.GetDifference(upstreamPackageId, downstreamPackageId);

            // get diff again, to hit cached version too. This is for coverage.
            diff = this.PackageDiffService.GetDifference(upstreamPackageId, downstreamPackageId);

            Assert.Equal(downstreamPackageId, diff.DownstreamPackageId);
            Assert.Equal(upstreamPackageId, diff.UpstreamPackageId);
            Assert.Single(diff.Difference);
            Assert.Equal(2, diff.Common.Count);
        }
    }
}
