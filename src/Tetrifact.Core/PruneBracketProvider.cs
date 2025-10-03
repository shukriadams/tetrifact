using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    public class PruneBracketProvider : IPruneBracketProvider
    {
        IEnumerable<PruneBracketProcess> IPruneBracketProvider.PruneBrackets { get; set; }

        public PruneBracketProvider(ISettings settings, TimeProvider timeProvider) 
        {
            // sort brackets newest to oldest, clone to change settings without affecting original instances
            IList<PruneBracketProcess> pruneBrackets = settings.PruneBrackets
                .Select(p => PruneBracketProcess.FromPruneBracket(p))
                .OrderBy(p => p.Days)
                .ToList();

            DateTime utcNow = timeProvider.GetUtcNow();
            DateTime ceiling = utcNow;
            foreach (PruneBracketProcess pruneBracketProcess in pruneBrackets)
            {
                pruneBracketProcess.StartUtc = ceiling;
                pruneBracketProcess.EndUtc = utcNow.AddDays(-1 * pruneBracketProcess.Days);
                ceiling = pruneBracketProcess.EndUtc;
            }

            ((IPruneBracketProvider)this).PruneBrackets = pruneBrackets;
        }

        PruneBracketProcess IPruneBracketProvider.MatchByDate(DateTime date) 
        {
            // get most recent bracket package falls into

            return ((IPruneBracketProvider)this).PruneBrackets
                .Where(bracket => bracket.Contains(date))
                .OrderBy(b => b.Days)
                .FirstOrDefault();
        }
    }
}
