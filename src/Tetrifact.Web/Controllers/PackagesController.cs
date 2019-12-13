using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;
using System.Threading.Tasks;

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
        private readonly ITagsService _tagService;

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
        public PackagesController(IPackageCreate packageService, ITagsService tagService, IPackageList packageList, IIndexReader indexService, ITetriSettings settings, ILogger<PackagesController> log)
        {
            _packageList = packageList;
            _packageService = packageService;
            _indexService = indexService;
            _settings = settings;
            _tagService = tagService;
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
        public JsonResult ListPackages([FromQuery(Name = "project")] string project, [FromQuery(Name = "isFull")] bool isFull, [FromQuery(Name = "index")] int pageIndex, [FromQuery(Name = "size")] int pageSize = 25)
        {
            if (isFull)
            {
                return new JsonResult(_packageList.Get(project, pageIndex, pageSize));
            }
            else
            {
                return new JsonResult(_indexService.GetPackageIds(project, pageIndex, pageSize));
            }
        }

        /// <summary>
        /// Gets latest package with the given tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("latest/{project}/{tag}")]
        public ActionResult<Package> GetLatestPackageWithTag(string project, string tag)
        {
            try
            {
                Package package = _packageList.GetLatestWithTag(project, tag);
                if (package == null)
                    return NotFound($"Couldn't find any packages tagged with \"{tag}\". Try another tag maybe?");

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
        [HttpGet("{project}/{package}/exists")]
        public ActionResult<bool> PackageExists(string project, string package)
        {
            return _indexService.GetManifest(project, package) != null;
        }


        /// <summary>
        /// Returns the manifest for a package
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{project}/{package}")]
        public ActionResult GetPackage(string project, string package)
        {
            try
            {
                Manifest manifest = _indexService.GetManifest(project, package);
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
        /// Handles posting a new package to system. 
        /// 
        /// Url : /packages/[ID]
        /// Method : POST
        /// Header
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpPost("{project}/{id}")]
        public async Task<ActionResult> AddPackage([FromForm]PackageCreateArguments post)
        {
            try
            {
                // check if there is space available
                DiskUseStats useStats = FileHelper.GetDiskUseSats();
                if (useStats.ToPercent() < _settings.SpaceSafetyThreshold)
                    return Responses.InsufficientSpace("Insufficient space on storage drive.");

                PackageCreateResult result = await _packageService.CreatePackage(post);
                if (result.Success)
                {
                    _packageList.Clear();
                    return Ok($"Success - package \"{post.Id}\" created.");
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
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpDelete("{project}/{package}")]
        public ActionResult DeletePackage(string project, string package)
        {
            try
            {
                _indexService.MarkPackageForDelete(project, package);
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