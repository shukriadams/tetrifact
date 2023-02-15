using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Web
{
    public class PackageExistingLookupFromPost
    {
        /// <summary>
        /// Files to upload to package (multipart/form-data).
        /// </summary>
        [FromForm] public string Manifest { get; set; }
    }
}
