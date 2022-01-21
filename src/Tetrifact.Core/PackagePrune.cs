using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    public class PackagePrune : IPackagePrune
    {
        #region FIELDS 

        ITetriSettings _settings;

        IIndexReader _indexReader;

        ILogger<IPackagePrune> _logger;

        #endregion

        public PackagePrune(ITetriSettings settings, IIndexReader indexReader, ILogger<IPackagePrune> logger)
        {
            _settings = settings;
            _indexReader = indexReader;
            _logger = logger;
        }

        private void CalculatePrune(IList<string> queue, ref IEnumerable<string> prune, int keep, string context)
        {
            if (queue.Count < keep || keep == 0)
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

        public void Prune()
        { 
            if (!_settings.Prune)
                return;

            IList<string> weeklyPruneQueue = new List<string>();
            IList<string> monthlyPruneQueue = new List<string>();
            IList<string> yearlyPruneQueue = new List<string>();
            IEnumerable<string> prune = new List<string>();

            IEnumerable<string> packageIds = _indexReader.GetAllPackageIds();
            DateTime? weeklyPruneFloor = _settings.PruneWeeklyThreshold != 0 ? DateTime.UtcNow.AddDays(-1 *_settings.PruneWeeklyThreshold) : (DateTime?)null;
            DateTime? monthlyPruneFloor = _settings.PruneMonthlyThreshold != 0 ? DateTime.UtcNow.AddDays(-1 * _settings.PruneMonthlyThreshold) : (DateTime?)null;
            DateTime? yearlyPrunedFloor = _settings.PruneYearlyThreshold != 0 ? DateTime.UtcNow.AddDays(-1 * _settings.PruneYearlyThreshold) : (DateTime?)null;

            foreach (string packageId in packageIds)
            {
                Manifest manifest = _indexReader.GetManifest(packageId);
                if (manifest == null)
                    continue;

                if (weeklyPruneFloor != null && manifest.CreatedUtc < weeklyPruneFloor)
                    weeklyPruneQueue.Add(packageId);
                else if (monthlyPruneFloor != null && manifest.CreatedUtc < monthlyPruneFloor)
                    monthlyPruneQueue.Add(packageId);
                else if (yearlyPrunedFloor != null && manifest.CreatedUtc < yearlyPrunedFloor)
                    yearlyPruneQueue.Add(packageId);
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
                    //_indexReader.DeletePackage(packageId);
                }
            }
        }
    }
}
