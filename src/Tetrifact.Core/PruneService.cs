using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    public class PruneService : IPruneService
    {
        #region FIELDS 

        ISettings _settings;

        IIndexReadService _indexReader;

        ILogger<IPruneService> _log;

        ITimeProvideer _timeprovider;

        IProcessManager _processManager;

        #endregion

        #region CTORS

        public PruneService(ISettings settings, IProcessManager processManager, ITimeProvideer timeprovider, IIndexReadService indexReader, ILogger<IPruneService> log)
        {
            _settings = settings;
            _indexReader = indexReader;
            _log = log;
            _timeprovider = timeprovider;
            _processManager = processManager;
        }

        #endregion

        #region METHODS

        public void Prune()
        {
            if (!_settings.PruneEnabled)
            {
                _log.LogInformation("Prune exited on start, disabled.");
                return;
            }

            if (_processManager.AnyWithCategoryExists(ProcessCategories.Package_Create))
            {
                IEnumerable<ProcessItem> locks = _processManager.GetAll();
                _log.LogInformation($"Prune exited on start, locks detected  : ({string.Join(", ", locks)}).");
                return;
            }

            PrunePlan report = this.GeneratePrunePlan();

            foreach(string line in report.Report)
               _log.LogInformation(line);

            IEnumerable<string> packageToPruneIds = report.Brackets.SelectMany(b => b.Prune).Select(m => m.Id);

            _log.LogInformation($"******************************* Starting prune execution, {packageToPruneIds.Count()} packages marked for delete *******************************");

            foreach (string packageId in packageToPruneIds)
            {
                try
                {
                    if (_settings.PruneDeletesEnabled) 
                    {
                        _indexReader.DeletePackage(packageId);
                        _log.LogInformation($"Pruned package {packageId}");
                    }
                    else 
                    {
                        _log.LogInformation($"Would have pruned package {packageId} (PruneDeletesEnabled is false)");
                    }
                } 
                catch (Exception ex)
                {
                    _log.LogError($"Prune failed for package {packageId} {ex}");
                }
            }

            _log.LogInformation("*************************************** Finished prune exectution. **************************************************************************");

            return;
        }

        public PrunePlan GeneratePrunePlan()
        {
            // sort brackets newest to oldest, clone to change settings without affecting original instances
            IList<PruneBracketProcess> processBrackets = _settings.PruneBrackets
                .Select(p => PruneBracketProcess.FromPruneBracket(p))
                .OrderBy(p => p.Days)
                .ToList();

            IList<string> taggedKeep = new List<string>();
            IList<string> report = new List<string>();

            int ignoringNoBracketCount = 0;

            IList<string> packageIds = _indexReader.GetAllPackageIds().ToList();
            packageIds = packageIds.OrderBy(n => Guid.NewGuid()).ToList(); // randomize collection order

            DateTime utcNow = _timeprovider.GetUtcNow();
            DateTime ceiling = utcNow;
            foreach(PruneBracketProcess pruneBracketProcess in processBrackets) 
            {
                pruneBracketProcess.Ceiling = ceiling;
                pruneBracketProcess.Floor = utcNow.AddDays(-1 * pruneBracketProcess.Days);
                ceiling = pruneBracketProcess.Floor;
            }

            int startingPackageCount = packageIds.Count;

            report.Add($"Server currently holds {packageIds.Count} packages.");

            foreach (string packageId in packageIds)
            {
                Manifest manifest = _indexReader.GetManifestHead(packageId);
                report.Add(string.Empty);

                if (manifest == null)
                {
                    _log.LogWarning($"Expected manifest for package {packageId} was not found, skipping.");
                    continue;
                }

                string flattenedTags = manifest.Tags.Count == 0 ? string.Empty : $"Tags : {string.Join(",", manifest.Tags)}";
                flattenedTags = string.IsNullOrEmpty(flattenedTags) ? "Package is untagged": $"Tagged with : {flattenedTags}";

                int ageInDays = (int)Math.Round((utcNow - manifest.CreatedUtc).TotalDays, 0);
                report.Add($"Analysing package \"{packageId}\", added {manifest.CreatedUtc.ToIso()} ({ageInDays} days ago). {flattenedTags}.");

                PruneBracketProcess matchingBracket = processBrackets.FirstOrDefault(bracket => bracket.Contains(manifest.CreatedUtc));
                if (matchingBracket == null)
                {
                    report.Add($"Package \"{packageId}\" doesn't fit into any prune brackets, will not be pruned.");
                    ignoringNoBracketCount ++;
                    continue;
                }

                report.Add($"Package \"{packageId}\" fits into prune bracket {matchingBracket}. This bracket currently contains {matchingBracket.Keep.Count} packages for keeping.");
                matchingBracket.Found++;

                // try to find reasons to keep package
                    
                // Bracket with Amount -1 means do not delete any
                if (matchingBracket.Amount == -1) 
                {
                    matchingBracket.Keep.Add(manifest);
                    report.Add($"Package \"{packageId}\" falls into bracket with no-prune (-1), keeping.");
                    continue;
                }

                // packages can be tagged to never be deleted. This ignores keep count, but will push out packages that are not tagged
                IEnumerable<string> keepTagsOnPackage = manifest.Tags.Where(tag => _settings.PruneIgnoreTags.Any(protectedTag => protectedTag.Equals(tag)));
                if (keepTagsOnPackage.Any())
                {
                    taggedKeep.Add(packageId);
                    matchingBracket.Keep.Add(manifest);
                    report.Add($"Package \"{packageId}\" marked for keep based on tag(s) {string.Join(",", keepTagsOnPackage)}.");
                    continue;
                }
                
                // "group strategy" - the entire bracket is treated as one big bag
                if (matchingBracket.Grouping == PruneBracketGrouping.Grouped && matchingBracket.Keep.Count < matchingBracket.Amount)
                {
                    report.Add($"Package \"{packageId}\" marked for keep based on its date grouping (bracket {matchingBracket}) {matchingBracket.Amount - matchingBracket.Keep.Count} slots left in bracket.");
                    matchingBracket.Keep.Add(manifest);
                    continue;
                }

                // bracket is on x packages per day basis
                if (matchingBracket.Grouping == PruneBracketGrouping.Daily) 
                {
                    int code = manifest.CreatedUtc.ToDayCode();
                    int kept = matchingBracket.Keep.Count(m => m.CreatedUtc.ToDayCode() == code);
                    if (kept < matchingBracket.Amount)
                    {
                        report.Add($"Package \"{packageId}\" marked for keep, bracket {matchingBracket}::day {code} had {matchingBracket.Amount - kept} slots left.");
                        matchingBracket.Keep.Add(manifest);
                        continue;
                    }
                }

                // bracket is on x packages per week basis
                if (matchingBracket.Grouping == PruneBracketGrouping.Weekly)
                {
                    int code = manifest.CreatedUtc.ToWeekCode();
                    int kept = matchingBracket.Keep.Count(m => m.CreatedUtc.ToWeekCode() == code);
                    if (kept < matchingBracket.Amount)
                    {
                        report.Add($"Package \"{packageId}\" marked for keep, bracket {matchingBracket}::week {code} had {matchingBracket.Amount - kept} slots left.");
                        matchingBracket.Keep.Add(manifest);
                        continue;
                    }
                }

                // bracket is on x packages per month basis
                if (matchingBracket.Grouping == PruneBracketGrouping.Monthly)
                {
                    int code = manifest.CreatedUtc.ToMonthCode();
                    int kept = matchingBracket.Keep.Count(m => m.CreatedUtc.ToMonthCode() == code);
                    if (kept < matchingBracket.Amount)
                    {
                        report.Add($"Package \"{packageId}\" marked for keep, bracket {matchingBracket}::month {code} had {matchingBracket.Amount - kept} slots left.");
                        matchingBracket.Keep.Add(manifest);
                        continue;
                    }
                }

                // bracket is on x packages per year basis
                if (matchingBracket.Grouping == PruneBracketGrouping.Yearly)
                {
                    int code = manifest.CreatedUtc.Year;
                    int kept = matchingBracket.Keep.Count(m => m.CreatedUtc.Year == code);
                    if (kept < matchingBracket.Amount)
                    {
                        report.Add($"Package \"{packageId}\" marked for keep, bracket {matchingBracket}::year {code} had {matchingBracket.Amount - kept} slots left.");
                        matchingBracket.Keep.Add(manifest);
                        continue;
                    }
                }

                // no reasons found, prune package
                matchingBracket.Prune.Add(manifest);
                report.Add($"Package \"{packageId}\" failed to pass any keep tests, marked for prune.");

            } // for each

            string pruneIdList = string.Empty;
            if (packageIds.Count > 0)
                pruneIdList = $" ({string.Join(",", packageIds)})";

            report.Add(string.Empty);

            int totalKeep = 0;
            int totalPrune = 0;

            foreach (PruneBracketProcess p in processBrackets)
            {
                totalKeep += p.Keep.Count;
                totalPrune += p.Prune.Count;
                report.Add($"Bracket {p}.");
                report.Add($"Found {p.Found} packages falling in this date range.");
                report.Add($"Keeping {p.Keep.Count}{FlattenList(p.Keep.Select(p => p.Id))}.");
                report.Add($"Pruning {p.Prune.Count}{FlattenList(p.Prune.Select(p => p.Id))}.");
                report.Add(string.Empty);
            }

            if (taggedKeep.Any())
                report.Add($"Kept {taggedKeep.Count} packages because of tag matches{FlattenList(taggedKeep)}.");
            else
                report.Add("No packages were kept due to tag matching. Note that packages need to fall into a bracket first before keep tagging rules are applied.");

            report.Add($"Total packages in system:{packageIds.Count}, no bracket match:{ignoringNoBracketCount}, pruning:{totalPrune}, keeping:{totalKeep}.");

            int totalHandled = totalKeep + totalPrune + ignoringNoBracketCount;
            if (packageIds.Count != totalHandled) 
                report.Add($"ERROR : Package handling count error, expected {packageIds.Count}, got {totalHandled}.");

            return new PrunePlan{ 
                Report = report, 
                Brackets = processBrackets
            };
        }

        private string FlattenList(IEnumerable<object> packages) 
        {
            if (!packages.Any())
                return string.Empty;

            return $" ({string.Join(",", packages)})";
        }

        #endregion
    }
}
