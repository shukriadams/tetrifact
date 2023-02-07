using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PartialPackageLookupResult
    {
        public IEnumerable<ManifestItem> Files { get; set; }

        public PartialPackageLookupResult()
        {
            this.Files = new List<ManifestItem>();
        }
    }
}
