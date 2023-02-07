using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PartialPackageLookupArguments
    {
        public IEnumerable<ManifestItem> Files { get;set;}

        public string Id { get; set; }

        public PartialPackageLookupArguments() 
        {
            this.Files = new List<ManifestItem>();
        }
    }
}
