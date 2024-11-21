using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PruneBracketProcess : PruneBracket
    {
        public IList<Manifest> Keep { get ; set; } = new List<Manifest>();

        public IList<Manifest> Prune { get; set; }  = new List<Manifest>();

        public TimeSpan Coverage { get; set; }

        /// <summary>
        /// Days back in time from Now that bracket covers. Calculated at start of a given prune run.
        /// </summary>
        public DateTime Floor { get; set; }

        public DateTime Ceiling { get; set; }

        public override string ToString()
        {
            return $"Ceiling:{this.Ceiling.ToIsoShort()} floor:{this.Floor.ToIsoShort()} (from {this.Ceiling.Ago()} ago to {this.Floor.Ago()} ago), {base.ToString()}";
        }

        public bool Contains(DateTime date) 
        {
            // note that we use compare only, not equal. Still trying to trace issues with packages being aggressively deleted by landing
            // in multiple brackets, for erring on side of caution. Packages that fail to match a bracket will always be kept.
            return date < this.Ceiling && date > this.Floor;
        }

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
