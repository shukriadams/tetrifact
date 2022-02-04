using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PackageDiff
    {
        public string UpstreamPackageId {get ; set; }

        public double Taken { get;set; }
        
        public string DownstreamPackageId { get; set; }

        public DateTime GeneratedOnUTC { get; set; }

        /// <summary>
        /// Files in which Packages A and B don't have in common
        /// </summary>
        public IList<ManifestItem> Files { get; set; }
    }
}
