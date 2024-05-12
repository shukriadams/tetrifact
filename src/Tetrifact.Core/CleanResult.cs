using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class CleanResult
    {
        public IEnumerable<string> Cleaned { get; set; }

        public IEnumerable<string> Failed { get; set; }

        public long DirectoriesScanned { get; set; }

        public long FilesScanned { get; set; }

        public int PackagesInSystem { get; set; }

        public string Description { get; set; }

        public CleanResult() 
        { 
            this.Cleaned = new List<string>();
            this.Failed = new List<string>();
        }
    }
}
