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

        ITimeProvider _timeprovider;

        IProcessManager _processManager;

        IPruneBracketProvider _pruneBracketProvider;

        private bool _hasRun;

        #endregion

        #region CTORS

        public PruneService(ISettings settings, IPruneBracketProvider pruneBracketProvider, IProcessManagerFactory processManagerFactory, ITimeProvider timeprovider, IIndexReadService indexReader, ILogger<IPruneService> log)
        {
            _settings = settings;
            _indexReader = indexReader;
            _log = log;
            _timeprovider = timeprovider;
            _processManager = processManagerFactory.GetInstance(ProcessManagerContext.Package_Create);
            _pruneBracketProvider = pruneBracketProvider;
        }

        #endregion

        #region METHODS

        public PrunePlan Prune()
        {
            // preven rerun on same _pruneBracketProvider instance, causes over-deleting of packages
            if (_hasRun)
                throw new Exception("Cannot reuse instance, generate new");

            _hasRun = true;

            if (!_settings.PruneEnabled)
            {
                _log.LogInformation("Prune exited on start, disabled.");
                return new PrunePlan { AbortDescription = "Prune exited on start, disabled." };
            }

            if (_processManager.AnyWithCategoryExists(ProcessCategories.Package_Create))
            {
                IEnumerable<ProcessItem> locks = _processManager.GetAll();
                _log.LogInformation($"Prune exited on start, locks detected  : ({string.Join(", ", locks)}).");
                return new PrunePlan { AbortDescription = $"Prune exited on start, locks detected  : ({string.Join(", ", locks)})." };
            }

            PrunePlan report = this.GeneratePrunePlan();

            foreach(string line in report.Report)
               _log.LogInformation(line);

            IEnumerable<string> packageToPruneIds = _pruneBracketProvider.PruneBrackets.SelectMany(b => b.Prune).Select(m => m.Id);

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

            _log.LogInformation("*************************************** Finished prune execution. **************************************************************************");

            return report;
        }

        public PrunePlan GeneratePrunePlan()
        {
            IList<string> taggedKeep = new List<string>();
            IList<string> report = new List<string>();

            int ignoringNoBracketCount = 0;

            IList<string> packageIds = _indexReader.GetAllPackageIds().ToList();
            packageIds = packageIds.OrderBy(n => Guid.NewGuid()).ToList(); // randomize collection order

            DateTime utcNow = _timeprovider.GetUtcNow();
            DateTime ceiling = utcNow;

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

                IEnumerable<string> keepTagsOnPackage = manifest.Tags.Where(tag => _settings.PruneIgnoreTags.Any(protectedTag => protectedTag.Equals(tag)));

                // get most recent bracket package falls into
                PruneBracketProcess matchingBracket = _pruneBracketProvider.MatchByDate(manifest.CreatedUtc);

                // package does not fit into a bracket
                if (matchingBracket == null)
                {
                    // there are later brackets which package will eventually fit into, therefore let it live
                    if (_pruneBracketProvider.PruneBrackets.Any(b => b.StartUtc < manifest.CreatedUtc))
                    {
                        report.Add($"Package \"{packageId}\" doesn't currently fit into any prune brackets, but will fit later. Keeping package.");
                        ignoringNoBracketCount++;
                        continue;
                    }

                    // there are no later brackets, but, stale deletes is disabled, so package lives on forever.
                    if (!_settings.DeleteStalePackages) 
                    {
                        report.Add($"Package \"{packageId}\" doesn't fit into any prune brackets, and there are no brackets existing brackets that will. Stale packages are not set to be deleted, so this package will be kept forever.");
                        ignoringNoBracketCount++;
                        continue;
                    }

                    // package is stale, but is protected with a keep tag, and keep tags override stale deletes unless explicitly overridden.
                    if (keepTagsOnPackage.Any() && !_settings.DeleteStalePackagesWithProtectedTags)
                    {
                        report.Add($"Package \"{packageId}\" doesn't fit into any prune brackets and is stale, but is protected with tag(s) {string.Join(",", keepTagsOnPackage)}. Package will be kept forever.");
                        ignoringNoBracketCount++;
                        continue;
                    }

                    // package is stale, unprotected and stale deletes are enabled. Delete.
                    matchingBracket.Prune.Add(manifest);
                    report.Add($"Package \"{packageId}\" is stale (too old to fit into any brackets). Will be deleted.");
                    continue;
                }

                // package does fit into a bracket, do bracket-fitting stuff
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

            foreach (PruneBracketProcess p in _pruneBracketProvider.PruneBrackets)
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
                Report = report
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
