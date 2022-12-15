using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    public class PackagePruneService : IPackagePruneService
    {
        #region FIELDS 

        ISettings _settings;

        IIndexReadService _indexReader;

        ILogger<IPackagePruneService> _logger;

        #endregion

        #region CTORS

        public PackagePruneService(ISettings settings, IIndexReadService indexReader, ILogger<IPackagePruneService> logger)
        {
            _settings = settings;
            _indexReader = indexReader;
            _logger = logger;
        }

        #endregion

        #region METHODS

        public void Prune()
        { 
            if (!_settings.Prune)
                return;

            IList<string> weeklyPruneQueue = new List<string>();
            IList<string> monthlyPruneQueue = new List<string>();
            IList<string> yearlyPruneQueue = new List<string>();
            IEnumerable<string> prune = new List<string>();

            IEnumerable<string> packageIds = _indexReader.GetAllPackageIds();

            // calculate periods for weekly, monthly and year pruning - weekly happens first, and after some time from NOW passes. Monthly starts at some point after weekly starts
            // and yearl starst some point after that and runs indefinitely.
            DateTime? weeklyPruneFloor = _settings.PruneWeeklyThreshold != 0 ? DateTime.UtcNow.AddDays(-1 *_settings.PruneWeeklyThreshold) : (DateTime?)null;
            DateTime? monthlyPruneFloor = _settings.PruneMonthlyThreshold != 0 ? DateTime.UtcNow.AddDays(-1 * _settings.PruneMonthlyThreshold) : (DateTime?)null;
            DateTime? yearlyPrunedFloor = _settings.PruneYearlyThreshold != 0 ? DateTime.UtcNow.AddDays(-1 * _settings.PruneYearlyThreshold) : (DateTime?)null;

            // assign all existing packages to either a yearly, monthly or weekly group.
            // Packages that are more recent than the weekly prune period are igored.
            // Packages with safe tags are ignored.
            foreach (string packageId in packageIds)
            {
                Manifest manifest = _indexReader.GetManifest(packageId);

                if (manifest == null){
                    _logger.LogWarning($"Expected manifest for package {packageId} was not found, skipping.");
                    continue;
                }

                if (manifest.Tags.Any(tag => _settings.PruneProtectectedTags.Any(protectedTag => protectedTag.Equals(tag))))
                    continue;

                if (yearlyPrunedFloor != null && manifest.CreatedUtc < yearlyPrunedFloor)
                    yearlyPruneQueue.Add(packageId);
                else if (monthlyPruneFloor != null && manifest.CreatedUtc < monthlyPruneFloor)
                    monthlyPruneQueue.Add(packageId);
                else if (weeklyPruneFloor != null && manifest.CreatedUtc < weeklyPruneFloor)
                    weeklyPruneQueue.Add(packageId);
            }

            // log out audit for prune, use warning because we expect this to be logged as important
            _logger.LogWarning(" ******************************** Prune audit **********************************");
            _logger.LogWarning($" Weekly prune keep is {_settings.PruneWeeklyKeep}. {weeklyPruneQueue.Count()} packages in pre-filter queue.");
            _logger.LogWarning($" Monthly prune keep is {_settings.PruneMonthlyKeep}. {monthlyPruneQueue.Count()} packages in pre-filter queue.");
            _logger.LogWarning($" Yearly prune keep is {_settings.PruneYearlyKeep}. {yearlyPruneQueue.Count()} packages in pre-filter queue.");

            // weekly
            this.CalculatePrune(weeklyPruneQueue, ref prune, _settings.PruneWeeklyKeep, "weekly");

            // monthly
            this.CalculatePrune(monthlyPruneQueue, ref prune, _settings.PruneMonthlyKeep, "monthly");

            // yearly
            this.CalculatePrune(yearlyPruneQueue, ref prune, _settings.PruneYearlyKeep, "yearly");

            _logger.LogWarning($"{weeklyPruneQueue.Count()} packages retained in post-filter weekly queue");
            _logger.LogWarning($"{monthlyPruneQueue.Count()} packages retained in post-filter monthly queue");
            _logger.LogWarning($"{yearlyPruneQueue.Count()} packages retained in post-filter yearly queue");
            _logger.LogWarning(" ******************************** Prune audit **********************************");

            if (prune.Count() > 0) 
            {
                foreach (string packageId in prune)
                {
                    try
                    {
                        _indexReader.DeletePackage(packageId);
                    } 
                    catch (Exception ex)
                    {
                        _logger.LogError($"Prune failed for package {packageId}", ex);
                    }
                }
            }
        }

        private void CalculatePrune(IList<string> queue, ref IEnumerable<string> prune, int keep, string context)
        {
            // keeping no builds is not supported - if set to zero, no pruning will be done for this period
            if (keep == 0)
                return;

            IEnumerable<string> take = queue
                // randomize
                .OrderBy(n => Guid.NewGuid())
                // take excess
                .Take(queue.Count - keep);

            _logger.LogWarning($"{take.Count()} packages of {queue.Count()} marked for delete in {context} prune. Ids are : {string.Join(",", take)}");
            prune = prune.Concat(take);
        }

        #endregion
    }
}
