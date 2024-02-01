using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Web
{
    public class PackageExistingLookupFromPost
    {
        /// <summary>
        /// Files to upload to package (multipart/form-data).
        /// </summary>
        [FromForm] public IFormFile Manifest { get; set; }
    }
}
