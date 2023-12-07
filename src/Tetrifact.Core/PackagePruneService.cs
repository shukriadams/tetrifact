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

        ITimeProvideer _timeprovider;

        #endregion

        #region CTORS

        public PackagePruneService(ISettings settings, ITimeProvideer timeprovider, IIndexReadService indexReader, ILogger<IPackagePruneService> logger)
        {
            _settings = settings;
            _indexReader = indexReader;
            _logger = logger;
            _timeprovider = timeprovider;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="keep"></param>
        /// <param name="keepIds"></param>
        /// <param name="packageIds"></param>
        /// <param name="isWeekly"></param>
        /// <returns></returns>
        private void RemoveKeep(ref IList<string> keepIds, ref IList<string> packageIds) 
        { 
            foreach (string id in keepIds)
                packageIds.Remove(id);
        }

        public void Prune()
        {
            if (!_settings.Prune)
                return;

            PruneReport report = this.Report();

            foreach(string line in report.Report)
               _logger.LogInformation(line);

            foreach (string packageId in report.PackageIds)
            {
                try
                {
                    if (_settings.DEBUG_block_prune_deletes)
                        _logger.LogInformation($"Would have pruned package {packageId} (DEBUG_block_prune_deletes enabled)");
                    else {
                        _indexReader.DeletePackage(packageId);
                        _logger.LogInformation($"Pruned package {packageId}");
                    }
                } 
                catch (Exception ex)
                {
                    _logger.LogError($"Prune failed for package {packageId}", ex);
                }
            }
        }

        public PruneReport Report()
        {
            IList<string> weeklyKeep = new List<string>();
            IList<string> weeklyPrune = new List<string>();
            IList<string> monthlyKeep = new List<string>();
            IList<string> monthlyPrune = new List<string>();
            IList<string> yearlyKeep = new List<string>();
            IList<string> yearlyPrune = new List<string>();
            IList<string> taggedKeep = new List<string>();
            IList<string> newKeep = new List<string>();
            IList<string> report = new List<string>();

            report.Add(" ******************************** Prune audit **********************************");

            IList<string> packageIds = _indexReader.GetAllPackageIds().ToList();
            packageIds = packageIds.OrderBy(n => Guid.NewGuid()).ToList(); // randomize

            // calculate periods for weekly, monthly and year pruning - weekly happens first, and after some time from NOW passes. Monthly starts at some point after weekly starts
            // and yearl starst some point after that and runs indefinitely.
            DateTime utcNow = _timeprovider.GetUtcNow();
            DateTime weeklyPruneFloor = utcNow.AddDays(-1 * _settings.PruneWeeklyThreshold);
            DateTime monthlyPruneFloor = utcNow.AddDays(-1 * _settings.PruneMonthlyThreshold);
            DateTime yearlyPruneFloor = utcNow.AddDays(-1 * _settings.PruneYearlyThreshold);
            int inWeeky = 0;
            int inMonthly = 0;
            int inYearly = 0;
            int startingPackageCount = packageIds.Count;

            if (_settings.DEBUG_block_prune_deletes)
                report.Add($"DEBUG_block_prune_deletes is enabled, packages will not be deleted");

            report.Add($"Found {packageIds.Count} packages :");

            foreach (string packageId in packageIds)
            {
                Manifest manifest = _indexReader.GetManifestHead(packageId);

                if (manifest == null)
                {
                    _logger.LogWarning($"Expected manifest for package {packageId} was not found, skipping.");
                    continue;
                }

                bool isTaggedKeep = manifest.Tags.Any(tag => _settings.PruneIgnoreTags.Any(protectedTag => protectedTag.Equals(tag)));
                string flattenedTags = manifest.Tags.Count == 0 ? string.Empty : $"Tags : {string.Join(",", manifest.Tags)}";
                int ageInDays = (int)Math.Round((utcNow - manifest.CreatedUtc).TotalDays, 0);
                report.Add($"- {packageId}, added {TimeHelper.ToIsoString(manifest.CreatedUtc)} ({ageInDays}) days ago). {flattenedTags}");

                if (isTaggedKeep){
                    taggedKeep.Add(packageId);
                    continue;
                }

                if (ageInDays > _settings.PruneYearlyThreshold)
                {
                    inYearly++;
                    if (yearlyKeep.Count < _settings.PruneYearlyKeep || isTaggedKeep)
                        yearlyKeep.Add(packageId);
                    else
                        yearlyPrune.Add(packageId);

                    continue;
                }

                if (ageInDays <= _settings.PruneYearlyThreshold && ageInDays > _settings.PruneMonthlyThreshold)
                {
                    inMonthly++;

                    if (monthlyKeep.Count < _settings.PruneMonthlyKeep || isTaggedKeep)
                        monthlyKeep.Add(packageId);
                    else
                        monthlyPrune.Add(packageId);

                    continue;
                }

                if (ageInDays <= _settings.PruneMonthlyThreshold && ageInDays > _settings.PruneWeeklyThreshold)
                {
                    inWeeky++;

                    if (weeklyKeep.Count < _settings.PruneWeeklyKeep || isTaggedKeep)
                        weeklyKeep.Add(packageId);
                    else
                        weeklyPrune.Add(packageId);

                    continue;
                }

                if (ageInDays <= _settings.PruneWeeklyThreshold)
                    newKeep.Add(packageId);
                
            }

            RemoveKeep(ref yearlyKeep, ref packageIds);
            RemoveKeep(ref monthlyKeep, ref packageIds);
            RemoveKeep(ref weeklyKeep, ref packageIds);
            RemoveKeep(ref newKeep, ref packageIds);
            RemoveKeep(ref taggedKeep, ref packageIds);

            string pruneIdList = string.Empty;
            if (packageIds.Count > 0)
                pruneIdList = $" ({string.Join(",", packageIds)})";

            string yearlyPruneFlattened = yearlyPrune.Count == 0 ? string.Empty : $" pruning {string.Join(",", yearlyPrune)}";
            string monthlyPruneFlattened = monthlyPrune.Count == 0 ? string.Empty : $" pruning {string.Join(",", monthlyPrune)}";
            string weeklyPruneFlattened = weeklyPrune.Count == 0 ? string.Empty : $" pruning {string.Join(",", weeklyPrune)}";

            // log out audit for prune, use warning because we expect this to be logged as important
            report.Add(string.Empty);
            report.Add($"Pre-weekly ignore count is {newKeep.Count()} - {string.Join(",", newKeep)}");
            if (taggedKeep.Count > 0)
                report.Add($"Kept due to tagging - {string.Join(",", taggedKeep)}.");
            report.Add($"WEEKLY prune (before {TimeHelper.ToIsoString(weeklyPruneFloor)}, {_settings.PruneWeeklyThreshold} days ago) count is {_settings.PruneWeeklyKeep}. Keeping {weeklyKeep.Count()} of {inWeeky}. {string.Join(",", weeklyKeep)}{weeklyPruneFlattened}.");
            report.Add($"MONTHLY prune (before {TimeHelper.ToIsoString(monthlyPruneFloor)}, {_settings.PruneMonthlyThreshold} days ago) count is {_settings.PruneMonthlyKeep}. Keeping {monthlyKeep.Count()} of {inMonthly}. {string.Join(",", monthlyKeep)}{monthlyPruneFlattened}.");
            report.Add($"YEARLY prune (before {TimeHelper.ToIsoString(yearlyPruneFloor)}, {_settings.PruneYearlyThreshold} days ago) count is {_settings.PruneYearlyKeep}. Keeping {yearlyKeep.Count()} of {inYearly}. {string.Join(",", yearlyKeep)}{yearlyPruneFlattened}.");
            report.Add(string.Empty);
            report.Add($"Pruning {packageIds.Count} packages{pruneIdList}.");
            report.Add(" ******************************** Prune audit **********************************");

            return new PruneReport{ 
                Report = report, 
                PackageIds = packageIds 
            };
        }

        #endregion
    }
}
