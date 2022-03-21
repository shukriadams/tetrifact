using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PackageDiff
    {
        #region PROPERTIES

        public string UpstreamPackageId {get ; set; }
        
        public string DownstreamPackageId { get; set; }

        public DateTime GeneratedOnUTC { get; set; }

        public IList<ManifestItem> Common { get; set; }

        /// <summary>
        /// Files in which Packages A and B don't have in common
        /// </summary>
        public IList<ManifestItem> Difference { get; set; }

        #endregion

        #region CTORS

        public PackageDiff()
        { 
            this.Common = new List<ManifestItem>();

            this.Difference = new List<ManifestItem>();
        }

        #endregion
    }
}
