using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    public class PackagePrune : IPackagePrune
    {
        #region FIELDS 

        ISettings _settings;

        IIndexReader _indexReader;

        ILogger<IPackagePrune> _logger;

        #endregion

        #region CTORS

        public PackagePrune(ISettings settings, IIndexReader indexReader, ILogger<IPackagePrune> logger)
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

            // weekly
            this.CalculatePrune(weeklyPruneQueue, ref prune, _settings.PruneWeeklyKeep, "weekly");

            // monthly
            this.CalculatePrune(monthlyPruneQueue, ref prune, _settings.PruneMonthlyKeep, "monthly");

            // yearly
            this.CalculatePrune(yearlyPruneQueue, ref prune, _settings.PruneYearlyKeep, "yearly");

            if (prune.Count() > 0) 
            {
                foreach (string packageId in prune)
                {
                    _logger.LogInformation($"Pruning package {packageId}");

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

            if (queue.Count < keep)
                return;

            IEnumerable<string> take = queue
                // randomize
                .OrderBy(n => Guid.NewGuid())
                // take excess
                .Take(queue.Count - keep);

            if (take.Count() > 0)
            {
                _logger.LogInformation($"Found {take.Count()} packages for {context} prune.");
                prune = prune.Concat(take);
            }
        }

        #endregion
    }
}
