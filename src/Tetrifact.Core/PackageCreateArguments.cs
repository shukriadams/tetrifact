using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        /// Name of project to add package to. The package will be automatically created it it doesn't yet exist.
        /// </summary>
        [FromRoute] public string Project { get; set; }

        /// <summary>
        /// Requested public Id of the package. Id cannot already exist. When live, id is taken from the route url.
        /// </summary>
        [FromRoute] public string Id { get; set; }

        /// <summary>
        /// Package to branch from. Optional. If null, the new pack will diff against head. If set, uploaded package will diff against branchFrom. 
        /// If set, uploaded package cannot become head. Use this feature to group build variations around clusters and reduce unnecessary "noise" in
        /// your main binary stream.
        /// </summary>
        [FromRoute] public string BranchFrom { get; set; }

        /// <summary>
        /// Files to upload to package (multipart/form-data).
        /// </summary>
        [FromForm] public IList<IFormFile> Files { get; set; }

        /// <summary>
        /// True if the querystring IsArchive is set to true. If true, Files will be treated as an archive and unpacked.
        /// Format 
        /// </summary>
        [FromQuery] public bool IsArchive { get; set; }

        /// <summary>
        /// Format of uploaded archive. Default is tar. Allowed values : "zip" and "gz".
        /// </summary>
        [FromQuery] public string Format { get; set; }

        /// <summary>
        /// Optional description for package
        /// </summary>
        [FromQuery] public string Description { get; set; }

        #endregion

        #region CTORS

        /// <summary>
        /// Default ctor
        /// </summary>
        public PackageCreateArguments()
        {
            this.Files = new List<IFormFile>();
            this.Format = string.Empty;
        }

        #endregion
    }
}
