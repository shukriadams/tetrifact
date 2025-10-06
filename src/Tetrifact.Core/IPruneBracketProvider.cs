using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type which generates applicable process prune brackets from some configuration source, and then 
    /// allows matching a date against those brackets to find a fit.
    /// </summary>
    public interface IPruneBracketProvider
    {
        IEnumerable<PruneBracketProcess> PruneBrackets { get; }

        void SetBrackets(IEnumerable<PruneBracket> brackets);

        PruneBracketProcess MatchByDate(DateTime date);
    }
}
