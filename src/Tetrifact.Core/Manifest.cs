using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Exposes the contents of a package. On filesystems, this is stored as manifest.json in the package root folder.
    /// </summary>
    public class Manifest
    {
        /// <summary>
        /// The hash of the combined hash of all items, sorted alphabetically by path.
        /// </summary>
        public string Hash { get; set; }
    
        /// <summary>
        /// Files in this package. Each file can be downloaded using its public id, and should be saved at its path.
        /// </summary>
        public IList<ManifestItem> Files { get; private set; }

        /// <summary>
        /// Combined size (bytes) of files (not linked files) in package.
        /// </summary>
        public long SizeOnDisk { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public HashSet<string> Tags { get; private set; }

        /// <summary>
        /// Size of files and linked files in package.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Optional free text field for manifest.
        /// </summary>
        public string Description { get; set; }

        #region CTORS

        public Manifest()
        {
            this.Files = new List<ManifestItem>();
            this.Tags = new HashSet<string>();
            this.CreatedUtc = DateTime.UtcNow;
            this.Description = String.Empty;
        }

        #endregion
    }
}
