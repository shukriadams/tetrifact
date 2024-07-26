using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PruneReport
    {
        public IEnumerable<string> Report {get;set; }
        public IEnumerable<PruneBracketProcess> Brackets { get;set; }

        public PruneReport()
        { 
            this.Report = new List<string>();
            this.Brackets = new List<PruneBracketProcess>();
        }
    }
}
