using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Holds the results of a calculted prune run. This can be executed to do the actual prune.
    /// </summary>
    public class PrunePlan
    {
        public IEnumerable<string> Report { get;set; }

        public string AbortDescription { get; set; }

        public string ReportAll 
        { 
            get 
            { 
                if (this.Report != null)
                    return string.Join("\r", this.Report);
                return string.Empty;
            } 
        }

        public PrunePlan()
        { 
            this.Report = new List<string>();
        }
    }
}
