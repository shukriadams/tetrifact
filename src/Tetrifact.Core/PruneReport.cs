using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PruneReport
    {
        public IEnumerable<string> PackageIds {get;set; }
        public IEnumerable<string> Report {get;set; }

        public PruneReport()
        { 
            this.Report = new List<string>();
            this.PackageIds = new List<string>();
        }
    }
}
