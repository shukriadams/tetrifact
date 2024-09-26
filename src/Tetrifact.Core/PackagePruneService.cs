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

        ILogger<IPackagePruneService> _log;

        ITimeProvideer _timeprovider;

        IProcessLockManager _processLock;

        #endregion

        #region CTORS

        public PackagePruneService(ISettings settings, IProcessLockManager processLock, ITimeProvideer timeprovider, IIndexReadService indexReader, ILogger<IPackagePruneService> log)
        {
            _settings = settings;
            _indexReader = indexReader;
            _log = log;
            _timeprovider = timeprovider;
            _processLock = processLock;
        }

        #endregion

        #region METHODS

        public void Prune()
        {
            if (!_settings.Prune)
            {
                _log.LogInformation("Prune exited on start, disabled.");
                return;
            }

            if (_processLock.IsAnyLocked(ProcessLockCategories.Package_Create))
            {
                IEnumerable<ProcessLockItem> locks = _processLock.GetCurrent();
                _log.LogInformation($"Prune exited on start, locks detected  : ({string.Join(", ", locks)}).");
                return;
            }

            PrunePlan report = this.GeneratePrunePlan();

            foreach(string line in report.Report)
               _log.LogInformation(line);

            IEnumerable<string> packageToPruneIds = report.Brackets.SelectMany(b => b.Prune);

            _log.LogInformation($"******************************* Starting prune execution, {packageToPruneIds.Count()} packages marked for delete *******************************");

            foreach (string packageId in packageToPruneIds)
            {
                try
                {
                    if (_settings.DEBUG_block_prune_deletes)
                        _log.LogInformation($"Would have pruned package {packageId} (DEBUG_block_prune_deletes enabled)");
                    else {
                        _indexReader.DeletePackage(packageId);
                        _log.LogInformation($"Pruned package {packageId}");
                    }
                } 
                catch (Exception ex)
                {
                    _log.LogError($"Prune failed for package {packageId} {ex}");
                }
            }

            _log.LogInformation("*************************************** Finished prune exectution. **************************************************************************");

        }

        public PrunePlan GeneratePrunePlan()
        {
            // sort brackets oldest to most recent, clone to change settings without affecting original instances
            IList<PruneBracketProcess> brackets = _settings.PruneBrackets
                .Select(p => PruneBracketProcess.Clone(p))
                .OrderByDescending(p => p.Days)
                .ToList();

            IList<string> taggedKeep = new List<string>();
            IList<string> newKeep = new List<string>();
            IList<string> report = new List<string>();
            int unhandled = 0;

            report.Add(" ******************************** Prune audit start **********************************");

            IList<string> packageIds = _indexReader.GetAllPackageIds().ToList();
            packageIds = packageIds.OrderBy(n => Guid.NewGuid()).ToList(); // randomize collection order

            // calculate periods for weekly, monthly and year pruning - weekly happens first, and after some time from NOW passes. Monthly starts at some point after weekly starts
            // and yearl starst some point after that and runs indefinitely.
            DateTime utcNow = _timeprovider.GetUtcNow();
            foreach(PruneBracketProcess pruneBracketProcess in brackets)
                pruneBracketProcess.Floor = utcNow.AddDays(-1 * pruneBracketProcess.Days);

            int startingPackageCount = packageIds.Count;

            report.Add($"Server currently contains {packageIds.Count} packages.");

            foreach (string packageId in packageIds)
            {
                Manifest manifest = _indexReader.GetManifestHead(packageId);

                if (manifest == null)
                {
                    _log.LogWarning($"Expected manifest for package {packageId} was not found, skipping.");
                    continue;
                }

                string flattenedTags = manifest.Tags.Count == 0 ? string.Empty : $"Tags : {string.Join(",", manifest.Tags)}";
                int ageInDays = (int)Math.Round((utcNow - manifest.CreatedUtc).TotalDays, 0);
                report.Add($"Analysing {packageId}, added {manifest.CreatedUtc.ToIso()} ({ageInDays} days ago). Tagged with: {flattenedTags}");

                PruneBracketProcess matchingBracket = brackets.FirstOrDefault(b => manifest.CreatedUtc < b.Floor);
                if (matchingBracket == null)
                {
                    report.Add($"{packageId}, created {manifest.CreatedUtc.ToIso()}, does not land in any prune bracket, will be kept.");
                    unhandled ++;
                }
                else
                {
                    report.Add($"{packageId}, created {manifest.CreatedUtc.ToIso()}, lands in prune bracket {matchingBracket.Days}Days.");
                    bool isTaggedKeep = manifest.Tags.Any(tag => _settings.PruneIgnoreTags.Any(protectedTag => protectedTag.Equals(tag)));

                    if (isTaggedKeep)
                    {
                        taggedKeep.Add(packageId);
                        matchingBracket.Keep.Add(packageId);
                        report.Add($"{packageId} marked for keep based on tag.");
                    }
                    else 
                    { 
                        if (matchingBracket.Keep.Count < matchingBracket.Amount)
                        {
                            report.Add($"{packageId} marked for keep, {matchingBracket.Keep.Count} packages kept so far.");
                            matchingBracket.Keep.Add(packageId);
                        }
                        else
                        {
                            matchingBracket.Prune.Add(packageId);
                            report.Add($"{packageId} marked for prune, {matchingBracket.Keep.Count} packages already kept.");
                        }
                    }
                }
            }

            string pruneIdList = string.Empty;
            if (packageIds.Count > 0)
                pruneIdList = $" ({string.Join(",", packageIds)})";

            report.Add(string.Empty);
            report.Add($"Pre-weekly ignore count is {newKeep.Count()} - {string.Join(",", newKeep)}");
            report.Add($"Unhandled: {unhandled}");
            
            if (taggedKeep.Count > 0)
                report.Add($"Kept due to tagging - {string.Join(",", taggedKeep)}.");
    
            foreach(PruneBracketProcess p in brackets)
                report.Add($"Bracket {p}, keeping {p.Keep.Count} packages ({string.Join(",",p.Keep)}), pruning {p.Prune.Count} packages ({string.Join(",", p.Prune)})");

            report.Add(string.Empty);
            report.Add(" ******************************** Prune audit end **********************************");

            return new PrunePlan{ 
                Report = report, 
                Brackets = brackets
            };
        }

        #endregion
    }
}
