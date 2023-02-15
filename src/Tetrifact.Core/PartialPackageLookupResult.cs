using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PartialPackageLookupResult
    {
        public IEnumerable<ManifestItem> Existing { get; set; }

        public PartialPackageLookupResult()
        {
            this.Existing = new List<ManifestItem>();
        }
    }
}
