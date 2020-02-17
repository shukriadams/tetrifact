using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// A unique collection of files which can be retrieved as a single unit.
    /// </summary>
    public class Package
    {
        #region PROPERTIES

        /// <summary>
        /// Internal id of package, guaranteed to be unique for an instance of a package. Name can be reused after a package is deleted, but UniqueId cannot be resurrected.
        /// </summary>
        public Guid UniqueId { get; set; }

        /// <summary>
        /// The unique, public name of the package, corresponds to folder name in "/packages" folder.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Hash of all file hashes in package. Files must be sorted alphabetically by full path + name, their hash strings concatenated and then the resulting string hashed.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Optional free text field for manifest. 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public HashSet<string> Tags { get; set; }

        /// <summary>
        /// True if content of manifest has been diffed against other packages.
        /// </summary>
        public DiffStates DiffState { get; set; }

        /// <summary>
        /// Files in this package. Each file can be downloaded using its public id, and should be saved at its path.
        /// </summary>
        public IList<PackageItem> Files { get; private set; }

        /// <summary>
        /// Combined size (bytes) of files (not linked files) in package.
        /// </summary>
        public long SizeOnDisk { get; set; }

        /// <summary>
        /// Size of files and linked files in package.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Optional - string id of package this package depends on.
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// Path on disk for this manifest file. This path will change if the manifest is changed.
        /// </summary>
        public string PathOnDisk { get; set; }

        /// <summary>
        /// Size of chunks (in bytes) for this package. 
        /// </summary>
        public long FileChunkSize { get; set; }

        /// <summary>
        /// Retrieves percentage saved with diff patching.
        /// </summary>
        public int CompressedPercent
        {
            get
            {
                if (this.SizeOnDisk == 0 || this.Size == 0)
                    return 0;

                decimal p = (decimal)this.SizeOnDisk / (decimal)this.Size;
                return (int)Math.Round((decimal)(p * 100), 0);
            }
        }

        #endregion

        #region CTORS

        public Package()
        {
            this.Tags = new HashSet<string>();
            this.CreatedUtc = DateTime.UtcNow;
            this.Files = new List<PackageItem>();
            this.Description = string.Empty;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Duplicates all fields which should remain constant when a package is repacked.
        /// </summary>
        /// <param name="package"></param>
        public void CopyTo(Package package) 
        {
            package.CreatedUtc = this.CreatedUtc;
            package.Description = this.Description;
            package.FileChunkSize = this.FileChunkSize;
            package.Hash = this.Hash;
            package.Name = this.Name;
            package.Parent = this.Parent;
            package.Size = this.Size;
            package.Tags = this.Tags;
            package.UniqueId = this.UniqueId;
        }

        #endregion
    }
}
