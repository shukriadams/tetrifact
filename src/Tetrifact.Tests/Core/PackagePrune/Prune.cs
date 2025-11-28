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
    public class Prune
    {
        private TestContext _testContext = new TestContext();
        
        private PackageHelper _packageHelper;

        private MoqHelper _moqHelper;
        
        public Prune()
        {
            _packageHelper = new PackageHelper(_testContext);
            _moqHelper = new MoqHelper(_testContext);
            ISettings settings = _testContext.Get<ISettings>();
            settings.PruneEnabled = true;
            settings.PruneIgnoreTags = new string[] { "keep" };
        }

        [Fact(DisplayName = "Should match date with the bracket that has the lowest day range")]
        public void PackageAssignTest() 
        {
            IPruneBracketProvider pruneBracketProvider = _testContext.Get<IPruneBracketProvider>();
            DateTime now = DateTime.UtcNow;
            
            pruneBracketProvider.SetBrackets(new List<PruneBracket> {  
                new PruneBracket { Days = 1 },
                new PruneBracket { Days = 10 },
                new PruneBracket { Days = 100 }
            });

            PruneBracketProcess matchedBracket = pruneBracketProvider.MatchByDate(now);
            Assert.Equal(1, matchedBracket.Days);
        }

        [Fact]
        public void HappyPath()
        {
            ISettings settings = _testContext.Get<ISettings>();
            settings.PruneBrackets = new List<PruneBracket>(){
                new PruneBracket{ Days=7, Amount = -1 },    // prune none
                new PruneBracket{ Days=31, Amount = 3 },    // leave 3
                new PruneBracket{ Days=364, Amount = 3 },   // leave 3
                new PruneBracket{ Days=999, Amount = 3 }    // leave 3
            };

            // create packages :
            // packages under week threshold, none of these should not be deleted
            _packageHelper.CreateNewPackageFiles("under-week-1");
            _packageHelper.CreateNewPackageFiles("under-week-2");
            _packageHelper.CreateNewPackageFiles("under-week-3");
            _packageHelper.CreateNewPackageFiles("under-week-4");
            _packageHelper.CreateNewPackageFiles("under-week-5");

            // packages above week threshold, two should be deleted
            _packageHelper.CreateNewPackageFiles("above-week-1");
            _packageHelper.CreateNewPackageFiles("above-week-2");
            _packageHelper.CreateNewPackageFiles("above-week-3");
            _packageHelper.CreateNewPackageFiles("above-week-4");
            _packageHelper.CreateNewPackageFiles("above-week-5");
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-week-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-8));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-week-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-9));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-week-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-10));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-week-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-11));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-week-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-12));

            // packages above month threshold, two of these should be deleted
            _packageHelper.CreateNewPackageFiles("above-month-1");
            _packageHelper.CreateNewPackageFiles("above-month-2");
            _packageHelper.CreateNewPackageFiles("above-month-3");
            _packageHelper.CreateNewPackageFiles("above-month-4");
            _packageHelper.CreateNewPackageFiles("above-month-5");
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-month-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-32));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-month-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-33));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-month-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-34));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-month-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-35));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-month-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-36));

            // packages above year threshold, two of these should be deleted
            _packageHelper.CreateNewPackageFiles("above-year-1");
            _packageHelper.CreateNewPackageFiles("above-year-2");
            _packageHelper.CreateNewPackageFiles("above-year-3");
            _packageHelper.CreateNewPackageFiles("above-year-4");
            _packageHelper.CreateNewPackageFiles("above-year-5");
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-year-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-366));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-year-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-367));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-year-3"), "CreatedUtc", DateTime.UtcNow.AddDays(-368));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-year-4"), "CreatedUtc", DateTime.UtcNow.AddDays(-369));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-year-5"), "CreatedUtc", DateTime.UtcNow.AddDays(-370));

            // prune multiple times to ensure that randomization doesn't lead to unintended deletes
            List<PrunePlan> plans = new List<PrunePlan>();
            for (int i = 0; i < 10; i++)
            {
                PruneService pruneService = _testContext.Get<PruneService>();
                plans.Add(pruneService.Prune());
            }

            IIndexReadService indexReader = _testContext.Get<IIndexReadService>();
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
            IIndexReadService indexReader = _testContext.Get<IIndexReadService>();

            // create 5 packages with date "now" (for real now)
            _packageHelper.CreateNewPackageFiles("1");
            _packageHelper.CreateNewPackageFiles("2");
            _packageHelper.CreateNewPackageFiles("3");
            _packageHelper.CreateNewPackageFiles("4");
            _packageHelper.CreateNewPackageFiles("5");

            // run pr
            ISettings settings = _testContext.Get<ISettings>();
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
                PruneBracketProvider pruneBracketProvider = _testContext.Get<PruneBracketProvider>("timeProvider", timeProvider.Object);
                PruneService packagePrune = _moqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { settings, indexReader, pruneBracketProvider, timeProvider.Object });
                packagePrune.Prune();
            }

            Assert.Equal(5, indexReader.GetAllPackageIds().Count());

            // shift time by 8 days to put packages into weekly bracket, 1 package should be deleted
            now = DateTime.UtcNow.AddDays(9);
            List<PrunePlan> prunes = new List<PrunePlan>();
            for (int i = 0; i < 10; i++)
            {
                PruneBracketProvider pruneBracketProvider = _testContext.Get<PruneBracketProvider>("timeProvider", timeProvider.Object);
                PruneService packagePrune = _moqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { settings, indexReader, pruneBracketProvider, timeProvider.Object });
                var p = packagePrune.Prune();
                prunes.Add(p);
            }

            Assert.Equal(4, indexReader.GetAllPackageIds().Count());

            // shift time by 32 days to put packages into monthly bracket, 1 more package should be deleted
            now = DateTime.UtcNow.AddDays(32);
            for (int i = 0; i < 10; i++)
            {
                PruneBracketProvider pruneBracketProvider = _testContext.Get<PruneBracketProvider>("timeProvider", timeProvider.Object);

                PruneService packagePrune = _moqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { settings, indexReader, pruneBracketProvider , timeProvider.Object });
                packagePrune.Prune();
            }

            Assert.Equal(3, indexReader.GetAllPackageIds().Count());

            // shift time by 366 days to put packages into yearly bracket, 1 more package should be deleted
            now = DateTime.UtcNow.AddDays(366);
            for (int i = 0; i < 10; i++) 
            {
                PruneBracketProvider pruneBracketProvider = _testContext.Get<PruneBracketProvider>("timeProvider", timeProvider.Object);
                PruneService packagePrune = _moqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { settings, indexReader, pruneBracketProvider, timeProvider.Object });
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
            ISettings settings = _testContext.Get<ISettings>();
            settings.PruneEnabled = false;

            IPruneService packagePrune = _testContext.Get<PruneService>();
            packagePrune.Prune();
        }


        /// <summary>
        /// Test coverage for graceful handling of missing manifest
        /// </summary>
        [Fact]
        public void Prune_Missing_Manifest()
        {
            // Settings.PruneWeeklyKeep = 0;
            Mock<IIndexReadService> mockedIndexReader = _moqHelper.Mock<IIndexReadService>();
            mockedIndexReader
                .Setup(r => r.GetManifest(It.IsAny<string>()))
                .Returns<Manifest>(null);

            // create package then delete its manifest
            _packageHelper.CreateNewPackageFiles("dummy");
            foreach (string manifestPath in _packageHelper.GetManifestPaths("dummy"))
                File.Delete(manifestPath);
            
            IPruneService mockedPruner = _moqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { mockedIndexReader.Object }); 
            mockedPruner.Prune();
        }

        /// <summary>
        /// Test coverage for graceful handling of unexpected exception on package delete
        /// </summary>
        [Fact]
        public void Prune_Delete_Exception()
        {
            Mock<IIndexReadService> mockedIndexReader = _moqHelper.CreateMockWithDependencies<IndexReadService>(new object[]{ }).As<IIndexReadService>();
            mockedIndexReader
                .Setup(r => r.DeletePackage(It.IsAny<string>()))
                .Callback(() => {
                    throw new Exception("some-error");
                });

            // create packages, force all to be eligable for delete
            _packageHelper.CreateNewPackageFiles("dummy1");
            _packageHelper.CreateNewPackageFiles("dummy2");
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("dummy1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("dummy2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            IPruneService mockedPruner = _moqHelper.CreateInstanceWithDependencies<PruneService>(new object[] { mockedIndexReader.Object }); 
            mockedPruner.Prune();
        }

        /// <summary>
        /// Ensure that packages with protected tag are never marked for pruning.
        /// </summary>
        [Fact]
        public void Prune_Protected_Tag()
        {
            ISettings settings = _testContext.Get<ISettings>();
            // two packages above week threshold, one of these should be deleted, but protect both with tags
            _packageHelper.CreateNewPackageFiles("above-week-1");
            _packageHelper.CreateNewPackageFiles("above-week-2");
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-week-1"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));
            JsonHelper.WriteValuetoRoot(_packageHelper.GetManifestPaths("above-week-2"), "CreatedUtc", DateTime.UtcNow.AddDays(-22));

            TagHelper.TagPackage(settings, "keep", "above-week-1");
            TagHelper.TagPackage(settings, "keep", "above-week-2");

            IPruneService packagePrune = _testContext.Get<PruneService>();
            packagePrune.Prune();

            IIndexReadService indexReader = _testContext.Get<IIndexReadService>();
            IEnumerable<string> packages = indexReader.GetAllPackageIds();

            Assert.Equal(2, packages.Count());
            Assert.Contains("above-week-1", packages);
            Assert.Contains("above-week-2", packages);
        }
    }
}
