using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Tetrifact.Web
{
    public class PackageCreateFromPost
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
        /// Files to upload to package (multipart/form-data).
        /// </summary>
        [FromForm] public IList<IFormFile> Files { get; set; }

        /// <summary>
        /// Special arg for webform creation.
        /// </summary>
        [FromQuery] public bool RemoveFirstDirectoryFromPath { get; set; }

        /// <summary>
        /// True if the querystring IsArchive is set to true. If true, Files will be treated as an archive and unpacked.
        /// Format 
        /// </summary>
        [FromQuery] public bool IsArchive { get; set; }

        /// <summary>
        /// Optional description for package
        /// </summary>
        [FromQuery] public string Description { get; set; }

        #endregion

        #region CTORS

        /// <summary>
        /// Default ctor
        /// </summary>
        public PackageCreateFromPost()
        {
            this.Files = new List<IFormFile>();
        }

        #endregion
    }
}
