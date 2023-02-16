using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Holds required data for adding a new package to system. Can be used as route data.
    /// </summary>
    public class PackageCreateArguments
    {
        #region FIELDS

        /// <summary>
        /// Requested public Id of the package. Id cannot already exist. When live, id is taken from the route url.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Files to upload to package (multipart/form-data).
        /// </summary>
        public IList<PackageCreateItem> Files { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ManifestItem> ExistingFiles { get; set; }

        /// <summary>
        /// True if the querystring IsArchive is set to true. If true, Files will be treated as an archive and unpacked.
        /// Format 
        /// </summary>
        public bool IsArchive { get; set; }

        /// <summary>
        /// Optional description for package
        /// </summary>
        public string Description { get; set; }

        #endregion

        #region CTORS

        /// <summary>
        /// Default ctor
        /// </summary>
        public PackageCreateArguments()
        {
            this.Files = new List<PackageCreateItem>();
        }

        #endregion
    }
}