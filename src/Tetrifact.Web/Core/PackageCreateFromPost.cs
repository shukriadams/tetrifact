using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Tetrifact.Web
{
    /// <summary>
    /// Holds required data for adding a new package to system. Can be used as route data.
    /// </summary>
    public class PackageCreateFromPost
    {
        #region FIELDS

        /// <summary>
        /// Requested public Id of the package. Id cannot already exist. When live, id is taken from the route url.
        /// </summary>
        [FromRoute] public string Id { get; set; }

        /// <summary>
        /// Files to upload to package (multipart/form-data).
        /// </summary>
        [FromForm] public IList<IFormFile> Files { get; set; }

        /// <summary>
        /// Optional. A list of files that we assume exists on the server, which should be included in the incoming package. This is used to do
        /// deduping on the client, before uploading. This might be preferable if bandwidth is limited.
        ///
        /// This field must be a JSON-formated string for existing files to link to, and must deserialize to IEnumerable<ManifestItem>.
        /// </summary>
        [FromForm] public IFormFile ExistingFiles { get; set; }

        /// <summary>
        /// True if the querystring IsArchive is set to true. If true, Files will be treated as an archive and unpacked.
        /// Format 
        /// </summary>
        [FromQuery] public bool IsArchive { get; set; }

        /// <summary>
        /// Format of uploaded archive. Default is tar. Allowed values : zip
        /// </summary>
        [FromQuery] public string Format { get; set; }

        /// <summary>
        /// Optional description for package
        /// </summary>
        [FromQuery] public string Description { get; set; }

        /// <summary>
        /// Special arg for webform creation.
        /// </summary>
        [FromQuery] public bool RemoveFirstDirectoryFromPath { get; set; }

        #endregion

        #region CTORS

        /// <summary>
        /// Default ctor
        /// </summary>
        public PackageCreateFromPost()
        {
            this.Files = new List<IFormFile>();
            this.Format = "zip";
        }

        #endregion
    }
}
