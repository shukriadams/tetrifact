using Moq;
using System;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageDiff
{
    public class PackageDiff : TestBase
    {
        public PackageDiff()
        {
            ISettings settings = TestContext.Get<ISettings>();
            settings.WorkerThreadCount = 1;
        }

        [Fact]
        public void HappyPath_SingleThread()
        {
            ISettings settings = TestContext.Get<ISettings>();

            settings.WorkerThreadCount = 1;
            IPackageDiffService diffService = this.TestContext.Get<IPackageDiffService>();
            //this.PackageDiffService = new PackageDiffService(settings, fileSystem, indexReader, MemoryCacheHelper.GetInstance(), this.Logger);

            string upstreamPackageId = PackageHelper.CreateNewPackage(new string[]{ "same content", "packege 1 content", "same content" } );
            string downstreamPackageId = PackageHelper.CreateNewPackage(new string[] { "same content", "packege 2 content", "same content" });

            // get diff
            diffService.GetDifference(upstreamPackageId, downstreamPackageId);

            // get diff again, to hit cached version too. This is for coverage.
            Core.PackageDiff diff = diffService.GetDifference(upstreamPackageId, downstreamPackageId);

            Assert.Equal(downstreamPackageId, diff.DownstreamPackageId);
            Assert.Equal(upstreamPackageId, diff.UpstreamPackageId);
            Assert.Single(diff.Difference);
            Assert.Equal(2, diff.Common.Count);
        }

        [Fact]
        public void HappyPath_MultiThread()
        {
            ISettings settings = TestContext.Get<ISettings>();
            // set thread count to 2, aka multi
            settings.WorkerThreadCount = 2;
            
            IPackageDiffService diffService = this.TestContext.Get<IPackageDiffService>();

            string upstreamPackageId = PackageHelper.CreateNewPackage(new [] { "same content", "packege 1 content", "same content" });
            string downstreamPackageId = PackageHelper.CreateNewPackage(new [] { "same content", "packege 2 content", "same content" });

            // get diff
            diffService.GetDifference(upstreamPackageId, downstreamPackageId);

            // get diff again, to hit cached version too. This is for coverage.
            Core.PackageDiff diff = diffService.GetDifference(upstreamPackageId, downstreamPackageId);

            Assert.Equal(downstreamPackageId, diff.DownstreamPackageId);
            Assert.Equal(upstreamPackageId, diff.UpstreamPackageId);
            Assert.Single(diff.Difference);
            Assert.Equal(2, diff.Common.Count);
        }

        /// <summary>
        /// coverage for exception handling
        /// </summary>
        [Fact]
        public void Json_Error_Read_Existing_Diff()
        {
            Mock<IFileSystem> fs = new Mock<IFileSystem>();
            // force existence of existing diff file
            fs.Setup(mq => mq.File.Exists(It.IsAny<string>()))
                .Returns(true);

            // force exception when reading diff file
            fs.Setup(mq => mq.File.ReadAllText(It.IsAny<string>()))
                .Callback(() => {
                    throw new Exception("some-error-123");
                });

            IPackageDiffService diffService = TestContext.Get<IPackageDiffService>("filesystem", fs.Object);

            string upstreamPackageId = PackageHelper.CreateNewPackage(new [] { "same content", "packege 1 content", "same content" });
            string downstreamPackageId = PackageHelper.CreateNewPackage(new [] { "same content", "packege 2 content", "same content" });

            Exception ex = Assert.Throws<Exception>(() => diffService.GetDifference(upstreamPackageId, downstreamPackageId));
            Assert.StartsWith("Unexpected error reading diff", ex.Message);
            Assert.Contains("some-error-123", ex.InnerException.Message);
        }

        /// <summary>
        /// coverage for exception handling
        /// </summary>
        [Fact]
        public void Diff_Write_Error()
        {
            Mock<IFileSystem> fs = new Mock<IFileSystem>();

            // spoof required method
            fs.Setup(mq => mq.Directory.CreateDirectory(It.IsAny<string>())).Callback(() => { });

            // force error when writing diff to disk
            fs.Setup(mq => mq.File.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => {
                    throw new Exception("some-error-123");
                });

            // spoof manifests in index reader, we will call these by id
            Mock<IIndexReadService> indexReader = new Mock<IIndexReadService>();
            indexReader.Setup(mq => mq.GetExpectedManifest("package-1")).Returns(PackageHelper.CreateInMemoryManifest());
            indexReader.Setup(mq => mq.GetExpectedManifest("package-2")).Returns(PackageHelper.CreateInMemoryManifest());

            IPackageDiffService diffService = TestContext.Get<IPackageDiffService>(
                "filesystem", fs.Object, 
                "indexReader", indexReader.Object);

            // get diff
            Exception ex = Assert.Throws<Exception>(()=>{ diffService.GetDifference("package-1", "package-2"); }) ;

            // ensure expected exception was thrown
            Assert.Contains("Unexpected error writing diff between packages", ex.Message);
            Assert.Contains("some-error-123", ex.InnerException.Message);
        }
    }
}
