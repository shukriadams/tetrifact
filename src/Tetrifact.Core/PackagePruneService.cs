﻿using Microsoft.Extensions.Logging;
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

            IList<string> weeklyKeep = new List<string>();
            IList<string> monthlyKeep = new List<string>();
            IList<string> yearlyKeep = new List<string>();
            IList<string> taggedKeep = new List<string>();
            IList<string> newKeep = new List<string>();

            IList<string> packageIds = _indexReader.GetAllPackageIds().ToList();
            packageIds = packageIds.OrderBy(n => Guid.NewGuid()).ToList(); // randomize

            // calculate periods for weekly, monthly and year pruning - weekly happens first, and after some time from NOW passes. Monthly starts at some point after weekly starts
            // and yearl starst some point after that and runs indefinitely.
            DateTime utcNow = _timeprovider.GetUtcNow();
            DateTime weeklyPruneFloor = utcNow.AddDays(-1 *_settings.PruneWeeklyThreshold);
            DateTime monthlyPruneFloor = utcNow.AddDays(-1 * _settings.PruneMonthlyThreshold);
            DateTime yearlyPruneFloor = utcNow.AddDays(-1 * _settings.PruneYearlyThreshold);
            int inWeeky = 0;
            int inMonthly = 0;
            int inYearly = 0;
            int startingPackageCount = packageIds.Count;

            foreach (string packageId in packageIds)
            {
                Manifest manifest = _indexReader.GetManifestHead(packageId);

                if (manifest == null)
                {
                    _logger.LogWarning($"Expected manifest for package {packageId} was not found, skipping.");
                    continue;
                }

                bool isTaggedKeep = manifest.Tags.Any(tag => _settings.PruneProtectectedTags.Any(protectedTag => protectedTag.Equals(tag)));
                
                if (manifest.CreatedUtc < yearlyPruneFloor)
                {
                    inYearly ++;
                    if (yearlyKeep.Count < _settings.PruneYearlyKeep || isTaggedKeep)
                        yearlyKeep.Add(packageId);
                }

                if (manifest.CreatedUtc > yearlyPruneFloor && manifest.CreatedUtc < monthlyPruneFloor) 
                {
                    inMonthly ++;
                    if (monthlyKeep.Count < _settings.PruneMonthlyKeep || isTaggedKeep)
                        monthlyKeep.Add(packageId);
                }

                if (manifest.CreatedUtc > monthlyPruneFloor && manifest.CreatedUtc < weeklyPruneFloor)
                {
                    inWeeky ++;
                    if (weeklyKeep.Count < _settings.PruneWeeklyKeep || isTaggedKeep)
                        weeklyKeep.Add(packageId);
                }

                if (manifest.CreatedUtc > weeklyPruneFloor)
                    newKeep.Add(packageId);

                if (isTaggedKeep)
                    taggedKeep.Add(packageId);
            }

            RemoveKeep(ref yearlyKeep, ref packageIds);
            RemoveKeep(ref monthlyKeep, ref packageIds);
            RemoveKeep(ref weeklyKeep, ref packageIds);
            RemoveKeep(ref newKeep, ref packageIds);
            RemoveKeep(ref taggedKeep, ref packageIds);

            string pruneIdList = string.Empty;
            if (packageIds.Count > 0)
                pruneIdList = $"({string.Join(",", pruneIdList)})";

            // log out audit for prune, use warning because we expect this to be logged as important
            _logger.LogWarning(" ******************************** Prune audit **********************************");
            _logger.LogWarning($" Pre-weekly ignore count is {newKeep.Count()} - {string.Join(",", newKeep)}");
            _logger.LogWarning($" Weekly prune (starting from {weeklyPruneFloor}) count is {_settings.PruneWeeklyKeep}. Keeping {weeklyKeep.Count()} of {inWeeky} {string.Join(",", weeklyKeep)}.");
            _logger.LogWarning($" Monthly prune (starting from {monthlyPruneFloor}) countis {_settings.PruneMonthlyKeep}. Keeping {monthlyKeep.Count()} of {inMonthly} {string.Join(",", monthlyKeep)}.");
            _logger.LogWarning($" Yearly prune (starting from {yearlyPruneFloor}) count is {_settings.PruneYearlyKeep}. Keeping {yearlyKeep.Count()} of {inYearly} {string.Join(",", yearlyKeep)}.");
            _logger.LogWarning($" Pruning {packageIds.Count} packages {pruneIdList}.");
            _logger.LogWarning(" ******************************** Prune audit **********************************");

            foreach (string packageId in packageIds)
            {
                try
                {
                    if (_settings.DEBUG_block_prune_deletes)
                        _logger.LogDebug($"Would have pruned package {packageId} (DEBUG_block_prune_deletes enabled)");
                    else {
                        _indexReader.DeletePackage(packageId);
                        _logger.LogWarning($"Pruned package {packageId}");
                    }
                } 
                catch (Exception ex)
                {
                    _logger.LogError($"Prune failed for package {packageId}", ex);
                }
            }
        }

        #endregion
    }
}
