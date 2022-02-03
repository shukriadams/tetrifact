using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PackageDiff
    {
        public string PackageA {get ; set; }

        public string PackageB { get; set; }

        /// <summary>
        /// Files in which Packages A and B don't have in common
        /// </summary>
        public IList<ManifestItem> Files { get; set; }
    }
}
