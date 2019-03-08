using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;

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
        private readonly ITetriSettings _settings;
        public IIndexReader IndexService;
        private ILogger<PackagesController> _log;
        private PackageService _packageService;
        private PackageList _packageList;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public PackagesController(PackageService packageService, PackageList packageList, ITetriSettings settings, IIndexReader indexService, ILogger<PackagesController> log)
        {
            _packageList = packageList;
            _packageService = packageService;
            _settings = settings;
            IndexService = indexService;
            _log = log;
        }


        /// <summary>
        /// Gets a page of 
        /// Gets an array of all package ids 
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public JsonResult ListPackages([FromQuery(Name = "isFull")] bool isFull, [FromQuery(Name = "index")] int pageIndex, [FromQuery(Name = "size")] int pageSize = 25)
        {
            if (isFull)
            {
                return new JsonResult(_packageList.Get(pageIndex, pageSize));
            }
            else
            {
                return new JsonResult(IndexService.GetPackageIds(pageIndex, pageSize));
            }
        }


        /// <summary>
        /// Returns 1 if the package exists, 0 if not
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [HttpGet("{packageId}/exists")]
        public ActionResult<int> PackageExists(string packageId)
        {
            return IndexService.GetManifest(packageId) == null ? 0 : 1;
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
        [HttpPost("{id}")]
        public async Task<ActionResult> AddPackage([FromForm]PackageAddArgs post)
        {
            try
            {
                PackageAddResult result = await _packageService.AddPackageAsync(post);
                if (result.Success)
                {
                    _packageList.Clear();
                    return Ok();
                }
                    

                if (result.ErrorType == PackageAddErrorTypes.InvalidArchiveFormat)
                    return Responses.InvalidArchiveFormatError(post.Format);

                if (result.ErrorType == PackageAddErrorTypes.InvalidFileCount)
                    return Responses.InvalidArchiveContent();

                if (result.ErrorType == PackageAddErrorTypes.PackageExists)
                    return Responses.PackageExistsError(post.Id);

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
        [HttpDelete("{packageId}")]
        public ActionResult DeletePackage(string packageId)
        {
            try
            {
                IndexService.DeletePackage(packageId);
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

    }
}