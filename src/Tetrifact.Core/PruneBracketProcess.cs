using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Prune bracket, with additional data structures for doing prunes.
    /// </summary>
    public class PruneBracketProcess : PruneBracket
    {
        #region PROPERTIES

        /// <summary>
        /// Packages to keep where those packages fall into the time period for this bracket.
        /// </summary>
        public IList<Manifest> Keep { get ; set; } = new List<Manifest>();

        /// <summary>
        /// Packages to delete where those packages fall into the time period for this bracket.
        /// </summary>
        public IList<Manifest> Prune { get; set; }  = new List<Manifest>();

        /// <summary>
        /// Number of found packages that fell into this bracket's time period.
        /// </summary>
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

        /// <summary>
        /// Returns true if the given data falls in the period covered by this bracket.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool Contains(DateTime date) 
        {
            // note that we use compare only, not equal. Still trying to trace issues with packages being aggressively deleted by landing
            // in multiple brackets, for erring on side of caution. Packages that fail to match a bracket will always be kept.
            return date < this.StartUtc && date > this.EndUtc;
        }

        /// <summary>
        /// Creates an instance from a config-level bracket.
        /// </summary>
        /// <param name="pruneBracket"></param>
        /// <returns></returns>
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
