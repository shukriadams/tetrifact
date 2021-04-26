using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Exposes the contents of a package. On filesystems, this is stored as manifest.json in the package root folder.
    /// </summary>
    public class Manifest : Package
    {
        #region PROPERTIES
    
        /// <summary>
        /// Files in this package. Each file can be downloaded using its public id, and should be saved at its path.
        /// </summary>
        public IList<ManifestItem> Files { get; set; }

        /// <summary>
        /// Combined size (bytes) of files (not linked files) in package.
        /// </summary>
        public long SizeOnDisk { get; set; }

        /// <summary>
        /// Size of files and linked files in package.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// If true, binary date will be compressed when written to disk.
        /// </summary>
        public bool IsCompressed { get; set; }

        #endregion

        #region CTORS

        public Manifest()
        {
            this.Files = new List<ManifestItem>();
        }

        #endregion
    }
}
