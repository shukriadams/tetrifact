using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Holds required data for adding a new package to system. Can be used as route data.
    /// </summary>
    public class PackageAddArgs
    {
        #region FIELDS

        /// <summary>
        /// Requested public Id of the package. Id cannot already exist. When live, id is taken from the route url.
        /// </summary>
        [FromRoute] public string Id { get; set; }

        /// <summary>
        /// Files to upload to package (multipart/form-data).
        /// </summary>
        [FromForm] public IEnumerable<IFormFile> Files { get; set; }


        /// <summary>
        /// True if the querystring IsArchive is set to true. If true, Files will be treated as an archive and unpacked.
        /// Format 
        /// </summary>
        [FromQuery] public bool IsArchive { get; set; }

        /// <summary>
        /// Format of uploaded archive. Default is tar. Allowed values : tar|zip
        /// </summary>
        [FromQuery] public string Format { get; set; }

        #endregion

        #region CTORS

        /// <summary>
        /// Default ctor
        /// </summary>
        public PackageAddArgs()
        {
            this.Format = "zip";
        }

        /// <summary>
        /// Testing shorthand ctor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="files"></param>
        public PackageAddArgs(string id, IEnumerable<IFormFile> files)
        {
            this.Id = id;
            this.Files = files;
            this.Format = "zip";
        }

        #endregion

    }
}
