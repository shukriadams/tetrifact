using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Holds the results of a calculted prune run. This can be executed to do the actual prune.
    /// </summary>
    public class PrunePlan
    {
        public IEnumerable<string> Report {get;set; }
        public IEnumerable<PruneBracketProcess> Brackets { get;set; }

        public PrunePlan()
        { 
            this.Report = new List<string>();
            this.Brackets = new List<PruneBracketProcess>();
        }
    }
}
