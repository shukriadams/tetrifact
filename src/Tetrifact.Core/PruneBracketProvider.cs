using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    public class PruneBracketProvider : IPruneBracketProvider
    {
        IEnumerable<PruneBracketProcess> _pruneBrackets;

        IEnumerable<PruneBracketProcess> IPruneBracketProvider.PruneBrackets { get { return _pruneBrackets; } }
        
        private readonly TimeProvider _timeProvider;

        public PruneBracketProvider(ISettings settings, TimeProvider timeProvider) 
        {
            _timeProvider = timeProvider;

            // sort brackets newest to oldest, clone to change settings without affecting original instances
            this.SetBrackets(settings.PruneBrackets);
        }

        public void SetBrackets(IEnumerable<PruneBracket> brackets)
        {
            IList<PruneBracketProcess> pruneBrackets = brackets
                .Select(p => PruneBracketProcess.FromPruneBracket(p))
                .OrderBy(p => p.Days)
                .ToList();

            DateTime utcNow = _timeProvider.GetUtcNow();
            DateTime ceiling = utcNow;

            foreach (PruneBracketProcess pruneBracketProcess in pruneBrackets)
            {
                pruneBracketProcess.StartUtc = ceiling;
                pruneBracketProcess.EndUtc = utcNow.AddDays(-1 * pruneBracketProcess.Days);
                ceiling = pruneBracketProcess.EndUtc;
            }

            _pruneBrackets = pruneBrackets;
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
