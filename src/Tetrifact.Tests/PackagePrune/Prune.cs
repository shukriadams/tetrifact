using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackagePrune
{
    public class Prune : FileSystemBase
    {
        private readonly IPackagePrune _packagePrune;
        private readonly TestLogger<IPackagePrune> _logger;

        public Prune()
        {
            _logger = new TestLogger<IPackagePrune>();
            Settings.Prune = true;
            Settings.PruneWeeklyKeep = 1;
            Settings.PruneMonthlyKeep = 1;
            Settings.PruneYearlyKeep = 1;

            _packagePrune = new Core.PackagePrune(this.Settings, this.IndexReader, _logger);
        }

        [Fact]
        public void HappyPath()
        {
            // create packages :
            // two packages under week threshold, these two should not be deleted
            PackageHelper.CreatePackage(Settings, "under-week-1");
            PackageHelper.CreatePackage(Settings, "under-week-2");

            // two packages above week threshold, one of these should be deleted
            PackageHelper.CreatePackage(Settings, "above-week-1");
            PackageHelper.CreatePackage(Settings, "above-week-2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "above-week-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "above-week-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            // two packages above month threshold, one of these should be deleted
            PackageHelper.CreatePackage(Settings, "above-month-1");
            PackageHelper.CreatePackage(Settings, "above-month-2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "above-month-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "above-month-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));


            // two packages above year threshold, one of these should be deleted
            PackageHelper.CreatePackage(Settings, "above-year-1");
            PackageHelper.CreatePackage(Settings, "above-year-2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "above-year-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-366));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "above-year-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-366));

            _packagePrune.Prune();

            IEnumerable<string> packages = IndexReader.GetAllPackageIds();

            Assert.Equal(5, packages.Count());
            Assert.Single(packages.Where(r => r.StartsWith("above-week-")));
            Assert.Single(packages.Where(r => r.StartsWith("above-month-")));
            Assert.Single(packages.Where(r => r.StartsWith("above-year-")));
        }

        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Prune_Disabled()
        {
            this.Settings.Prune = false;
            _packagePrune.Prune();
        }

        /// <summary>
        /// Prune not done if keep set to zero.
        /// </summary>
        [Fact]
        public void Prune_No_Zero()
        {
            Settings.PruneWeeklyKeep = 0;
            // create packages and set dates to hit weekly prune threshold
            PackageHelper.CreatePackage(Settings, "above-week-1");
            PackageHelper.CreatePackage(Settings, "above-week-2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "above-week-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "above-week-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            _packagePrune.Prune();
            IEnumerable<string> packages = IndexReader.GetAllPackageIds();

            // ensure no packages were pruned
            Assert.Equal(2, packages.Count());
        }

        /// <summary>
        /// Test coverage for graceful handling of missing manifest
        /// </summary>
        [Fact]
        public void Prune_Missing_Manifest()
        {
            Settings.PruneWeeklyKeep = 0;

            MockRepository repository = new MockRepository(MockBehavior.Loose) { DefaultValue = DefaultValue.Mock, CallBase = true };
            Mock<Core.IndexReader> mockedIndexReader = repository.Create<Core.IndexReader>(Settings, ThreadDefault, TagService, Logger, FileSystem, HashServiceHelper.Instance());
            mockedIndexReader
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Returns<Manifest>(null);

            // create package then delete its manifest
            PackageHelper.CreatePackage(Settings, "dummy");
            File.Delete(PackageHelper.GetManifestPath(Settings, "dummy"));
            
            IPackagePrune mockedPruner = new Core.PackagePrune(this.Settings, mockedIndexReader.Object, _logger);
            mockedPruner.Prune();
        }

        /// <summary>
        /// Test coverage for graceful handling of unexpected exception on package delete
        /// </summary>
        [Fact]
        public void Prune_Delete_Exception()
        {
            MockRepository repository = new MockRepository(MockBehavior.Loose) { DefaultValue = DefaultValue.Mock, CallBase = true };
            Mock<Core.IndexReader> mockedIndexReader = repository.Create<Core.IndexReader>(Settings, ThreadDefault, TagService, Logger, FileSystem, HashServiceHelper.Instance());
            mockedIndexReader
                .Setup(r => r.DeletePackage(It.IsAny<string>()))
                .Callback(() => {
                    throw new Exception("some-error");
                });

            // create packages, force all to be eligable for delete
            PackageHelper.CreatePackage(Settings, "dummy1");
            PackageHelper.CreatePackage(Settings, "dummy2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "dummy1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPath(Settings, "dummy2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            IPackagePrune mockedPruner = new Core.PackagePrune(this.Settings, mockedIndexReader.Object, _logger);
            mockedPruner.Prune();
        }
    }
}
