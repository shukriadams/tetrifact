using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PruneBracketProcess : PruneBracket
    {
        public IList<Manifest> Keep {get ; set; } = new List<Manifest>();

        public IList<Manifest> Prune { get; set; }  = new List<Manifest>();

        public DateTime Floor { get; set; }

        public static PruneBracketProcess FromPruneBracket(PruneBracket pruneBracket) 
        { 
            return new PruneBracketProcess 
            { 
                Amount = pruneBracket.Amount,
                Days = pruneBracket.Days
            };
        }
    }
}
