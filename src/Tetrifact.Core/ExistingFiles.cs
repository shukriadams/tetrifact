using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class ExistingFiles
    {
        public IList<ManifestItem> Files { get; set; }

        public ExistingFiles() 
        { 
            this.Files = new List<ManifestItem>();
        }
    }
}
