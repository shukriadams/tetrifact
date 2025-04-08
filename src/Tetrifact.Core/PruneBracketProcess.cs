using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PruneBracketProcess : PruneBracket
    {
        #region PROPERTIES

        public IList<Manifest> Keep { get ; set; } = new List<Manifest>();

        public IList<Manifest> Prune { get; set; }  = new List<Manifest>();

        public int Found { get; set; }

        /// <summary>
        /// Days back in time from Now that bracket covers. Calculated at start of a given prune run.
        /// </summary>
        public DateTime EndUtc { get; set; }

        /// <summary>
        /// Going back in time, .StartUtc is when a bracket begins, it then runs for .Days back, until it reaches .EndUTc
        /// </summary>
        public DateTime StartUtc { get; set; }

        #endregion

        #region METHODS

        /// <summary>
        /// String representation of bracket. 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Ceiling:{this.StartUtc.ToIsoShort()} floor:{this.EndUtc.ToIsoShort()} (from {this.StartUtc.Ago()} ago to {this.EndUtc.Ago()} ago), {base.ToString()}";
        }

        public bool Contains(DateTime date) 
        {
            // note that we use compare only, not equal. Still trying to trace issues with packages being aggressively deleted by landing
            // in multiple brackets, for erring on side of caution. Packages that fail to match a bracket will always be kept.
            return date < this.StartUtc && date > this.EndUtc;
        }

        public static PruneBracketProcess FromPruneBracket(PruneBracket pruneBracket) 
        { 
            return new PruneBracketProcess 
            { 
                Amount = pruneBracket.Amount,
                Days = pruneBracket.Days,
                Grouping = pruneBracket.Grouping
            };
        }

        #endregion
    }
}
