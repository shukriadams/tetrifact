using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;
using System.Diagnostics;

namespace Tetrifact.Web
{

    /// <summary>
    /// - Creates and serves archives of packages.
    /// - Archive files are created on demand - this can take a while for large packages, so there
    /// can be a long wait time before the standard get method returns.
    /// - To save disk space, older archive files are cleaned out if space is limited.
    /// - For systems that cannot wait for the standard get to return, use the /order endpoint. This
    /// immediately returns a code for archive status.
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class PackagesController : Controller
    {
        #region FIELDS

        private readonly IIndexReader _indexService;
        private readonly ILogger<PackagesController> _log;
        private readonly IPackageCreate _packageService;
        private readonly IPackageList _packageList;
        private readonly ITetriSettings _settings;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="settings"></param>
        /// <param name="log"></param>
        public PackagesController(IPackageCreate packageService, IPackageList packageList, IIndexReader indexService, ITetriSettings settings, ILogger<PackagesController> log)
        {
            _packageList = packageList;
            _packageService = packageService;
            _indexService = indexService;
            _settings = settings;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Gets a page of packages
        /// Gets an array of all package ids 
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("")]
        public JsonResult ListPackages([FromQuery(Name = "isFull")] bool isFull, [FromQuery(Name = "index")] int pageIndex, [FromQuery(Name = "size")] int pageSize = 25)
        {
            if (isFull)
            {
                return new JsonResult(_packageList.Get(pageIndex, pageSize));
            }
            else
            {
                return new JsonResult(_indexService.GetPackageIds(pageIndex, pageSize));
            }
        }


        /// <summary>
        /// Gets latest package with the given tag.
        /// </summary>
        /// <param name="tags">Comma-separated string of tags</param>
        /// <returns>Package for the lookup.Null if no match.</returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("latest/{tags}")]
        public ActionResult<Package> GetLatestPackageWithTag(string tags)
        {
            try
            {
                string[] tagsSplit = tags.Split(",", StringSplitOptions.RemoveEmptyEntries);
                Package package = _packageList.GetLatestWithTags(tagsSplit);
                if (package == null)
                    return NotFound($"Couldn't find any packages tagged with \"{tags}\". Try another tag maybe?");

                return package;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                Console.WriteLine("An unexpected error occurred : ");
                Console.WriteLine(ex);
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Get rid of this, use GetPackage instead
        /// Returns 1 if the package exists, 0 if not
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{packageId}/exists")]
        public ActionResult<bool> PackageExists(string packageId)
        {
            return _indexService.GetManifest(packageId) != null;
        }


        /// <summary>
        /// Returns the manifest for a package
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{packageId}")]
        public ActionResult GetPackage(string packageId)
        {
            try
            {
                Manifest manifest = _indexService.GetManifest(packageId);
                if (manifest == null)
                    return NotFound();

                return new JsonResult(manifest);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                Console.WriteLine("An unexpected error occurred : ");
                Console.WriteLine(ex);
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Creates a new package. Returns JSON with local hash of package.
        /// 
        /// Url : /packages/[ID]
        /// Method : POST
        /// Header
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpPost("{id}")]
        public ActionResult AddPackage([FromForm]PackageCreateArguments post)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {

                // check if there is space available
                DiskUseStats useStats = FileHelper.GetDiskUseSats();
                if (useStats.ToPercent() < _settings.SpaceSafetyThreshold)
                    return Responses.InsufficientSpace("Insufficient space on storage drive.");

                PackageCreateResult result = _packageService.CreatePackage(post);
                if (result.Success)
                {
                    // force flush in-memory list of packages
                    _packageList.Clear();

                    return Ok(new
                    {
                        success = new
                        {
                            id = post.Id,
                            hash = result.PackageHash,
                            description = "Package successfully created"
                        }
                    });
                }

                if (result.ErrorType == PackageCreateErrorTypes.InvalidArchiveFormat)
                    return Responses.InvalidArchiveFormatError(post.Format);

                if (result.ErrorType == PackageCreateErrorTypes.InvalidFileCount)
                    return Responses.InvalidArchiveContent();

                if (result.ErrorType == PackageCreateErrorTypes.PackageExists)
                    return Responses.PackageExistsError(post.Id);

                if (result.ErrorType == PackageCreateErrorTypes.MissingValue)
                    return Responses.MissingInputError(result.PublicError);

                return Responses.UnexpectedError();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                Console.WriteLine("An unexpected error occurred : ");
                Console.WriteLine(ex);
                return Responses.UnexpectedError();
            }
            finally 
            {
                sw.Stop();
                _log.LogInformation($"Uploaded for package {post.Id} took {0} seconds", sw.Elapsed.TotalSeconds);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpDelete("{packageId}")]
        public ActionResult DeletePackage(string packageId)
        {
            try
            {
                _indexService.DeletePackage(packageId);
                _packageList.Clear();
                return Ok();
            }
            catch (PackageNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                Console.WriteLine("An unexpected error occurred : ");
                Console.WriteLine(ex);
                return Responses.UnexpectedError();
            }
        }

        #endregion
    }
}