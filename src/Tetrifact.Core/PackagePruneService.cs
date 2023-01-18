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
            DateTime weeklyPruneFloor = DateTime.UtcNow.AddDays(-1 *_settings.PruneWeeklyThreshold);
            DateTime monthlyPruneFloor = DateTime.UtcNow.AddDays(-1 * _settings.PruneMonthlyThreshold);
            DateTime yearlyPruneFloor = DateTime.UtcNow.AddDays(-1 * _settings.PruneYearlyThreshold);



            //PopulateKeep(yearlyPrunedFloor, _settings.PruneYearlyKeep, ref keep, ref packageIds);
            //PopulateKeep(monthlyPruneFloor, _settings.PruneYearlyKeep + _settings.PruneMonthlyKeep, ref keep, ref packageIds);
            //PopulateKeep(weeklyPruneFloor, _settings.PruneWeeklyKeep + _settings.PruneYearlyKeep + _settings.PruneMonthlyKeep, ref keep, ref packageIds, true );

            foreach (string packageId in packageIds)
            {
                Manifest manifest = _indexReader.GetManifest(packageId);

                if (manifest == null)
                {
                    _logger.LogWarning($"Expected manifest for package {packageId} was not found, skipping.");
                    continue;
                }

                bool isTaggedKeep = manifest.Tags.Any(tag => _settings.PruneProtectectedTags.Any(protectedTag => protectedTag.Equals(tag)));
                
                if (manifest.CreatedUtc < yearlyPruneFloor)
                {
                    if (yearlyKeep.Count < _settings.PruneYearlyKeep || isTaggedKeep)
                        yearlyKeep.Add(packageId);
                }

                if (manifest.CreatedUtc > yearlyPruneFloor && manifest.CreatedUtc < monthlyPruneFloor) 
                {
                    if (monthlyKeep.Count < _settings.PruneMonthlyKeep || isTaggedKeep)
                        monthlyKeep.Add(packageId);
                }

                if (manifest.CreatedUtc > monthlyPruneFloor && manifest.CreatedUtc < weeklyPruneFloor)
                {
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

            // log out audit for prune, use warning because we expect this to be logged as important
            _logger.LogWarning(" ******************************** Prune audit **********************************");
            _logger.LogWarning($" Weekly prune keep is {_settings.PruneWeeklyKeep}, keeping {weeklyKeep.Count()} - {string.Join(",", weeklyKeep)}");
            _logger.LogWarning($" Monthly prune keep is {_settings.PruneMonthlyKeep}, keeping {monthlyKeep.Count()} - {string.Join(",", monthlyKeep)}");
            _logger.LogWarning($" Yearly prune keep is {_settings.PruneYearlyKeep}, keeping {yearlyKeep.Count()} - {string.Join(",", yearlyKeep)}");
            _logger.LogWarning(" ******************************** Prune audit **********************************");

            foreach (string packageId in packageIds)
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

        #endregion
    }
}
