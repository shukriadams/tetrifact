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
        private readonly IPackagePruneService _packagePrune;

        public Prune()
        {
            Settings.Prune = true;
            Settings.PruneWeeklyThreshold = 7;
            Settings.PruneMonthlyThreshold = 31;
            Settings.PruneYearlyThreshold = 364;
            Settings.PruneWeeklyKeep = 3;
            Settings.PruneMonthlyKeep = 3;
            Settings.PruneYearlyKeep = 3;
            Settings.PruneIgnoreTags = new string[] { "keep" };

            _packagePrune = MoqHelper.CreateInstanceWithDependencies<PackagePruneService>(new object[]{ this.Settings, this.IndexReader }); 
        }

        [Fact]
        public void HappyPath()
        {
            // create packages :
            // packages under week threshold, none of these should not be deleted
            PackageHelper.CreateNewPackageFiles(Settings, "under-week-1");
            PackageHelper.CreateNewPackageFiles(Settings, "under-week-2");
            PackageHelper.CreateNewPackageFiles(Settings, "under-week-3");
            PackageHelper.CreateNewPackageFiles(Settings, "under-week-4");
            PackageHelper.CreateNewPackageFiles(Settings, "under-week-5");

            // packages above week threshold, two should be deleted
            PackageHelper.CreateNewPackageFiles(Settings, "above-week-1");
            PackageHelper.CreateNewPackageFiles(Settings, "above-week-2");
            PackageHelper.CreateNewPackageFiles(Settings, "above-week-3");
            PackageHelper.CreateNewPackageFiles(Settings, "above-week-4");
            PackageHelper.CreateNewPackageFiles(Settings, "above-week-5");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            // packages above month threshold, two of these should be deleted
            PackageHelper.CreateNewPackageFiles(Settings, "above-month-1");
            PackageHelper.CreateNewPackageFiles(Settings, "above-month-2");
            PackageHelper.CreateNewPackageFiles(Settings, "above-month-3");
            PackageHelper.CreateNewPackageFiles(Settings, "above-month-4");
            PackageHelper.CreateNewPackageFiles(Settings, "above-month-5");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));

            // packages above year threshold, two of these should be deleted
            PackageHelper.CreateNewPackageFiles(Settings, "above-year-1");
            PackageHelper.CreateNewPackageFiles(Settings, "above-year-2");
            PackageHelper.CreateNewPackageFiles(Settings, "above-year-3");
            PackageHelper.CreateNewPackageFiles(Settings, "above-year-4");
            PackageHelper.CreateNewPackageFiles(Settings, "above-year-5");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-366));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-466));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-566));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-666));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-766));

            // prune multiple times to ensure that randomization doesn't lead to unintended deletes
            for (int i = 0 ; i < 10 ; i ++)
                _packagePrune.Prune();

            IEnumerable<string> packages = IndexReader.GetAllPackageIds();

            Assert.Equal(14, packages.Count());

            Assert.Equal(3, packages.Where(r => r.StartsWith("above-week-")).Count());
            Assert.Equal(3, packages.Where(r => r.StartsWith("above-month-")).Count());
            Assert.Equal(3, packages.Where(r => r.StartsWith("above-year-")).Count());
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Prune_over_time() 
        {
            // create 5 packages with date "now" (for real now)
            PackageHelper.CreateNewPackageFiles(Settings, "1");
            PackageHelper.CreateNewPackageFiles(Settings, "2");
            PackageHelper.CreateNewPackageFiles(Settings, "3");
            PackageHelper.CreateNewPackageFiles(Settings, "4");
            PackageHelper.CreateNewPackageFiles(Settings, "5");

            // run pr
            Core.Settings settings = new Core.Settings();
            settings.Prune = true;
            settings.PruneWeeklyThreshold = 7;
            settings.PruneMonthlyThreshold = 31;
            settings.PruneYearlyThreshold = 364;
            settings.PruneWeeklyKeep = 4;
            settings.PruneMonthlyKeep = 3;
            settings.PruneYearlyKeep = 2;

            // mock time time provider to return a shifting "now" date
            DateTime now = DateTime.UtcNow;
            Mock<ITimeProvideer> timeProvider = new Mock<ITimeProvideer>();
            timeProvider.Setup(r => r.GetUtcNow())
                .Returns(()=> now);

            PackagePruneService packagePrune = MoqHelper.CreateInstanceWithDependencies<PackagePruneService>(new object[] { settings, this.IndexReader, timeProvider.Object });

            // prune now - now packages must be deleted
            for (int i = 0; i < 10; i++)
                packagePrune.Prune();
            Assert.Equal(5, this.IndexReader.GetAllPackageIds().Count());

            // shift time by 8 days to put packages into weekly bracket, 1 package should be deleted
            now = DateTime.UtcNow.AddDays(8);
            for (int i = 0; i < 10; i++)
                packagePrune.Prune();
            Assert.Equal(4, this.IndexReader.GetAllPackageIds().Count());

            // shift time by 32 days to put packages into monthly bracket, 1 more package should be deleted
            now = DateTime.UtcNow.AddDays(32);
            for (int i = 0; i < 10; i++)
                packagePrune.Prune();
            Assert.Equal(3, this.IndexReader.GetAllPackageIds().Count());

            // shift time by 365 days to put packages into yearly bracket, 1 more package should be deleted
            now = DateTime.UtcNow.AddDays(365);
            for (int i = 0; i < 10; i++)
                packagePrune.Prune();
            Assert.Equal(2, this.IndexReader.GetAllPackageIds().Count());
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
        /// Test coverage for graceful handling of missing manifest
        /// </summary>
        [Fact]
        public void Prune_Missing_Manifest()
        {
            Settings.PruneWeeklyKeep = 0;
            Mock<IIndexReadService> mockedIndexReader = MoqHelper.CreateMockWithDependencies<IndexReadService, IIndexReadService>(new object[]{ Settings, TagService, IndexReaderLogger, FileSystem, HashServiceHelper.Instance(), LockProvider });
            mockedIndexReader
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Returns<Manifest>(null);

            // create package then delete its manifest
            PackageHelper.CreateNewPackageFiles(Settings, "dummy");
            foreach (string manifestPath in PackageHelper.GetManifestPaths(Settings, "dummy"))
                File.Delete(manifestPath);
            
            IPackagePruneService mockedPruner = MoqHelper.CreateInstanceWithDependencies<PackagePruneService>(new object[] { this.Settings, mockedIndexReader.Object }); 
            mockedPruner.Prune();
        }

        /// <summary>
        /// Test coverage for graceful handling of unexpected exception on package delete
        /// </summary>
        [Fact]
        public void Prune_Delete_Exception()
        {
            Mock<IIndexReadService> mockedIndexReader = MoqHelper.CreateMockWithDependencies<IndexReadService>(new object[]{ Settings, TagService, IndexReaderLogger, FileSystem, HashServiceHelper.Instance(), LockProvider }).As<IIndexReadService>();
            mockedIndexReader
                .Setup(r => r.DeletePackage(It.IsAny<string>()))
                .Callback(() => {
                    throw new Exception("some-error");
                });

            // create packages, force all to be eligable for delete
            PackageHelper.CreateNewPackageFiles(Settings, "dummy1");
            PackageHelper.CreateNewPackageFiles(Settings, "dummy2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "dummy1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "dummy2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            IPackagePruneService mockedPruner = MoqHelper.CreateInstanceWithDependencies<PackagePruneService>(new object[] { this.Settings, mockedIndexReader.Object }); 
            mockedPruner.Prune();
        }

        /// <summary>
        /// Esnure that packages with protected tag are never marked for pruning.
        /// </summary>
        [Fact]
        public void Prune_Protected_Tag()
        {
            // two packages above week threshold, one of these should be deleted, but protect both with tags
            PackageHelper.CreateNewPackageFiles(Settings, "above-week-1");
            PackageHelper.CreateNewPackageFiles(Settings, "above-week-2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            TagHelper.TagPackage(Settings, "keep", "above-week-1");
            TagHelper.TagPackage(Settings, "keep", "above-week-2");
            _packagePrune.Prune();

            IEnumerable<string> packages = IndexReader.GetAllPackageIds();

            Assert.Equal(2, packages.Count());
            Assert.Contains("above-week-1", packages);
            Assert.Contains("above-week-2", packages);
        }
    }
}
