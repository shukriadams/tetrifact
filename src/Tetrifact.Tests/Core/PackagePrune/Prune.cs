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
        private readonly IPruneService _packagePrune;

        public Prune()
        {
            Settings.PruneEnabled = true;

            Settings.PruneIgnoreTags = new string[] { "keep" };

            _packagePrune = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[]{ Settings, this.IndexReader }); 
        }

        [Fact]
        public void HappyPath()
        {
            Settings.PruneBrackets = new List<PruneBracket>(){
                new PruneBracket{ Days=7, Amount = 3 },
                new PruneBracket{ Days=31, Amount = 3 },
                new PruneBracket{ Days=365, Amount = 3 }
            };

            // create packages :
            // packages under week threshold, none of these should not be deleted
            PackageHelper.CreateNewPackageFiles("under-week-1");
            PackageHelper.CreateNewPackageFiles("under-week-2");
            PackageHelper.CreateNewPackageFiles("under-week-3");
            PackageHelper.CreateNewPackageFiles("under-week-4");
            PackageHelper.CreateNewPackageFiles("under-week-5");

            // packages above week threshold, two should be deleted
            PackageHelper.CreateNewPackageFiles("above-week-1");
            PackageHelper.CreateNewPackageFiles("above-week-2");
            PackageHelper.CreateNewPackageFiles("above-week-3");
            PackageHelper.CreateNewPackageFiles("above-week-4");
            PackageHelper.CreateNewPackageFiles("above-week-5");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-week-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            // packages above month threshold, two of these should be deleted
            PackageHelper.CreateNewPackageFiles("above-month-1");
            PackageHelper.CreateNewPackageFiles("above-month-2");
            PackageHelper.CreateNewPackageFiles("above-month-3");
            PackageHelper.CreateNewPackageFiles("above-month-4");
            PackageHelper.CreateNewPackageFiles("above-month-5");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-month-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-91));

            // packages above year threshold, two of these should be deleted
            PackageHelper.CreateNewPackageFiles("above-year-1");
            PackageHelper.CreateNewPackageFiles("above-year-2");
            PackageHelper.CreateNewPackageFiles("above-year-3");
            PackageHelper.CreateNewPackageFiles("above-year-4");
            PackageHelper.CreateNewPackageFiles("above-year-5");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-366));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-466));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-566));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-666));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "above-year-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-766));

            // prune multiple times to ensure that randomization doesn't lead to unintended deletes
            for (int i = 0 ; i < 10 ; i ++)
                _packagePrune.Prune();

            IEnumerable<string> packages = IndexReader.GetAllPackageIds();

            Assert.Equal(14, packages.Count()); // 5 + 3 + 3 + 3 

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
            PackageHelper.CreateNewPackageFiles("1");
            PackageHelper.CreateNewPackageFiles("2");
            PackageHelper.CreateNewPackageFiles("3");
            PackageHelper.CreateNewPackageFiles("4");
            PackageHelper.CreateNewPackageFiles("5");

            // run pr
            ISettings settings = Settings;
            settings.PruneEnabled = true;
            Settings.PruneBrackets = new List<PruneBracket>(){
                new PruneBracket{ Days=7, Amount = 4 },
                new PruneBracket{ Days=31, Amount = 3 },
                new PruneBracket{ Days=365, Amount = 2 }
            };

            // mock time provider to return a fixed "now" date, we will be changing "now" ass we go along
            DateTime now = DateTime.UtcNow;
            Mock<ITimeProvideer> timeProvider = new Mock<ITimeProvideer>();
            timeProvider.Setup(r => r.GetUtcNow())
                .Returns(()=> now);

            PruneService packagePrune = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { settings, this.IndexReader, timeProvider.Object });

            // prune - no packages must be deleted
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

            // shift time by 366 days to put packages into yearly bracket, 1 more package should be deleted
            now = DateTime.UtcNow.AddDays(366);
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
            Settings.PruneEnabled = false;
            _packagePrune.Prune();
        }


        /// <summary>
        /// Test coverage for graceful handling of missing manifest
        /// </summary>
        [Fact]
        public void Prune_Missing_Manifest()
        {
            // Settings.PruneWeeklyKeep = 0;
            Mock<IIndexReadService> mockedIndexReader = MoqHelper.CreateMockWithDependencies<IndexReadService, IIndexReadService>(new object[]{ Settings, TagService, IndexReaderLogger, FileSystem, HashServiceHelper.Instance() });
            mockedIndexReader
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Returns<Manifest>(null);

            // create package then delete its manifest
            PackageHelper.CreateNewPackageFiles("dummy");
            foreach (string manifestPath in PackageHelper.GetManifestPaths(Settings, "dummy"))
                File.Delete(manifestPath);
            
            IPruneService mockedPruner = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { Settings, mockedIndexReader.Object }); 
            mockedPruner.Prune();
        }

        /// <summary>
        /// Test coverage for graceful handling of unexpected exception on package delete
        /// </summary>
        [Fact]
        public void Prune_Delete_Exception()
        {
            Mock<IIndexReadService> mockedIndexReader = MoqHelper.CreateMockWithDependencies<IndexReadService>(new object[]{ Settings, TagService, IndexReaderLogger, FileSystem, HashServiceHelper.Instance() }).As<IIndexReadService>();
            mockedIndexReader
                .Setup(r => r.DeletePackage(It.IsAny<string>()))
                .Callback(() => {
                    throw new Exception("some-error");
                });

            // create packages, force all to be eligable for delete
            PackageHelper.CreateNewPackageFiles("dummy1");
            PackageHelper.CreateNewPackageFiles("dummy2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "dummy1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(Settings, "dummy2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            IPruneService mockedPruner = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { Settings, mockedIndexReader.Object }); 
            mockedPruner.Prune();
        }

        /// <summary>
        /// Esnure that packages with protected tag are never marked for pruning.
        /// </summary>
        [Fact]
        public void Prune_Protected_Tag()
        {
            // two packages above week threshold, one of these should be deleted, but protect both with tags
            PackageHelper.CreateNewPackageFiles("above-week-1");
            PackageHelper.CreateNewPackageFiles("above-week-2");
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
