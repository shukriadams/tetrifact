using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PruneBracketProcess : PruneBracket
    {
        public IList<string> Keep {get ; set; } = new List<string>();

        public IList<string> Prune { get; set; }  = new List<string>();

        public DateTime Floor { get; set; }

        public static PruneBracketProcess Clone(PruneBracket pruneBracket) 
        { 
            return new PruneBracketProcess 
            { 
                Amount = pruneBracket.Amount,
                Days = pruneBracket.Days
            };
        }
    }
}
