using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public PackagesController(PackageService packageService, ITetriSettings settings, IIndexReader indexService, ILogger<PackagesController> log)
        {
            _packageService = packageService;
            _settings = settings;
            IndexService = indexService;
            _log = log;
        }


        /// <summary>
        /// Gets an array of all package ids 
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public JsonResult ListPackages()
        {
            _log.LogWarning("some trace");
            _log.LogTrace("some warning");
            _log.LogError(new Exception("Some exception"), "it really failed");

            IEnumerable<string> ids = IndexService.GetPackages();
            return new JsonResult(ids);
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
                    return Ok();

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