using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackagePrune
{
    public class Prune : TestBase
    {
        private readonly IPruneService _packagePrune;

        public Prune()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            settings.PruneEnabled = true;
            settings.PruneIgnoreTags = new string[] { "keep" };
            _packagePrune = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[]{ settings, indexReader }); 
        }

        [Fact]
        public void HappyPath()
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();
            ISettings settings = TestContext.Get<ISettings>();
            settings.PruneBrackets = new List<PruneBracket>(){
                new PruneBracket{ Days=7, Amount = -1 },    // prune none
                new PruneBracket{ Days=31, Amount = 3 },    // leave 3
                new PruneBracket{ Days=364, Amount = 3 },   // leave 3
                new PruneBracket{ Days=999, Amount = 3 }    // leave 3
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
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-week-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-8));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-week-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-9));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-week-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-10));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-week-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-11));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-week-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-12));

            // packages above month threshold, two of these should be deleted
            PackageHelper.CreateNewPackageFiles("above-month-1");
            PackageHelper.CreateNewPackageFiles("above-month-2");
            PackageHelper.CreateNewPackageFiles("above-month-3");
            PackageHelper.CreateNewPackageFiles("above-month-4");
            PackageHelper.CreateNewPackageFiles("above-month-5");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-month-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-32));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-month-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-33));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-month-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-34));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-month-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-35));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-month-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-36));

            // packages above year threshold, two of these should be deleted
            PackageHelper.CreateNewPackageFiles("above-year-1");
            PackageHelper.CreateNewPackageFiles("above-year-2");
            PackageHelper.CreateNewPackageFiles("above-year-3");
            PackageHelper.CreateNewPackageFiles("above-year-4");
            PackageHelper.CreateNewPackageFiles("above-year-5");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-year-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-366));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-year-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-367));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-year-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-368));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-year-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-369));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-year-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-370));

            // prune multiple times to ensure that randomization doesn't lead to unintended deletes
            List<PrunePlan> plans = new List<PrunePlan>();
            for (int i = 0; i < 10; i++)
            {
                PruneService pruneService = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { indexReader });
                plans.Add(pruneService.Prune());
            }

            IEnumerable<string> packages = indexReader.GetAllPackageIds();

            Assert.Equal(5, packages.Where(r => r.StartsWith("under-week-")).Count());
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
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            // create 5 packages with date "now" (for real now)
            PackageHelper.CreateNewPackageFiles("1");
            PackageHelper.CreateNewPackageFiles("2");
            PackageHelper.CreateNewPackageFiles("3");
            PackageHelper.CreateNewPackageFiles("4");
            PackageHelper.CreateNewPackageFiles("5");

            // run pr
            ISettings settings = TestContext.Get<ISettings>();
            settings.PruneEnabled = true;
            settings.PruneBrackets = new List<PruneBracket>(){
                new PruneBracket{ Days=7, Amount = -1 },
                new PruneBracket{ Days=31, Amount = 4 },
                new PruneBracket{ Days=365, Amount = 3 },
                new PruneBracket{ Days=999, Amount = 2 }
            };

            // mock time provider to return a fixed "now" date, we will be changing "now" ass we go along
            DateTime now = DateTime.UtcNow;
            Mock<TimeProvider> timeProvider = new Mock<TimeProvider>();
            timeProvider.Setup(r => r.GetUtcNow())
                .Returns(()=> now);


            // prune - no packages must be deleted
            
            for (int i = 0; i < 10; i++) 
            {
                PruneBracketProvider pruneBracketProvider = TestContext.Get<PruneBracketProvider>("timeProvider", timeProvider.Object);
                PruneService packagePrune = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { settings, indexReader, pruneBracketProvider, timeProvider.Object });
                packagePrune.Prune();
            }

            Assert.Equal(5, indexReader.GetAllPackageIds().Count());

            // shift time by 8 days to put packages into weekly bracket, 1 package should be deleted
            now = DateTime.UtcNow.AddDays(9);
            List<PrunePlan> prunes = new List<PrunePlan>();
            for (int i = 0; i < 10; i++)
            {
                PruneBracketProvider pruneBracketProvider = TestContext.Get<PruneBracketProvider>("timeProvider", timeProvider.Object);
                PruneService packagePrune = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { settings, indexReader, pruneBracketProvider, timeProvider.Object });
                var p = packagePrune.Prune();
                prunes.Add(p);
            }

            Assert.Equal(4, indexReader.GetAllPackageIds().Count());

            // shift time by 32 days to put packages into monthly bracket, 1 more package should be deleted
            now = DateTime.UtcNow.AddDays(32);
            for (int i = 0; i < 10; i++)
            {
                PruneBracketProvider pruneBracketProvider = TestContext.Get<PruneBracketProvider>("timeProvider", timeProvider.Object);

                PruneService packagePrune = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { settings, indexReader, pruneBracketProvider , timeProvider.Object });
                packagePrune.Prune();
            }

            Assert.Equal(3, indexReader.GetAllPackageIds().Count());

            // shift time by 366 days to put packages into yearly bracket, 1 more package should be deleted
            now = DateTime.UtcNow.AddDays(366);
            for (int i = 0; i < 10; i++) 
            {
                PruneBracketProvider pruneBracketProvider = TestContext.Get<PruneBracketProvider>("timeProvider", timeProvider.Object);
                PruneService packagePrune = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { settings, indexReader, pruneBracketProvider, timeProvider.Object });
                packagePrune.Prune();
            }

            Assert.Equal(2, indexReader.GetAllPackageIds().Count());
        }

        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Prune_Disabled()
        {
            ISettings settings = TestContext.Get<ISettings>();
            settings.PruneEnabled = false;
            _packagePrune.Prune();
        }


        /// <summary>
        /// Test coverage for graceful handling of missing manifest
        /// </summary>
        [Fact]
        public void Prune_Missing_Manifest()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();

            // Settings.PruneWeeklyKeep = 0;
            Mock<IIndexReadService> mockedIndexReader = MoqHelper.CreateMockWithDependencies<IndexReadService, IIndexReadService>(new object[]{ fileSystem, HashServiceHelper.Instance() });
            mockedIndexReader
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Returns<Manifest>(null);

            // create package then delete its manifest
            PackageHelper.CreateNewPackageFiles("dummy");
            foreach (string manifestPath in PackageHelper.GetManifestPaths("dummy"))
                File.Delete(manifestPath);
            
            IPruneService mockedPruner = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { mockedIndexReader.Object }); 
            mockedPruner.Prune();
        }

        /// <summary>
        /// Test coverage for graceful handling of unexpected exception on package delete
        /// </summary>
        [Fact]
        public void Prune_Delete_Exception()
        {
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();

            Mock<IIndexReadService> mockedIndexReader = MoqHelper.CreateMockWithDependencies<IndexReadService>(new object[]{ fileSystem, HashServiceHelper.Instance() }).As<IIndexReadService>();
            mockedIndexReader
                .Setup(r => r.DeletePackage(It.IsAny<string>()))
                .Callback(() => {
                    throw new Exception("some-error");
                });

            // create packages, force all to be eligable for delete
            PackageHelper.CreateNewPackageFiles("dummy1");
            PackageHelper.CreateNewPackageFiles("dummy2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("dummy1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("dummy2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            IPruneService mockedPruner = MoqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { mockedIndexReader.Object }); 
            mockedPruner.Prune();
        }

        /// <summary>
        /// Ensure that packages with protected tag are never marked for pruning.
        /// </summary>
        [Fact]
        public void Prune_Protected_Tag()
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();
            ISettings settings = TestContext.Get<ISettings>();
            // two packages above week threshold, one of these should be deleted, but protect both with tags
            PackageHelper.CreateNewPackageFiles("above-week-1");
            PackageHelper.CreateNewPackageFiles("above-week-2");
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-week-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths("above-week-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            TagHelper.TagPackage(settings, "keep", "above-week-1");
            TagHelper.TagPackage(settings, "keep", "above-week-2");
            _packagePrune.Prune();

            IEnumerable<string> packages = indexReader.GetAllPackageIds();

            Assert.Equal(2, packages.Count());
            Assert.Contains("above-week-1", packages);
            Assert.Contains("above-week-2", packages);
        }
    }
}
