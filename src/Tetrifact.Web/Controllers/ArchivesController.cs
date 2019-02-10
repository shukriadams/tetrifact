using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ArchivesController : Controller
    {
        private readonly ITetriSettings _settings;
        public IIndexReader IndexService;
        private ILogger<ArchivesController> _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public ArchivesController(ITetriSettings settings, IIndexReader indexService, ILogger<ArchivesController> log)
        {
            _settings = settings;
            IndexService = indexService;
            _log = log;
        }

        /// <summary>
        /// Gets an archive, starts its creation if archive doesn't exist. Returns when archive is available. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{packageId}")]
        public async Task<ActionResult> GetArchive(string packageId)
        {
            try
            {
                IndexService.PurgeOldArchives();

                return File(await IndexService.GetPackageAsArchiveAsync(packageId), "application/octet-stream", string.Format("{0}.zip", packageId));
            }
            catch (PackageNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                Console.WriteLine(ex);
                return Responses.UnexpectedError();
            }
        }

        /// <summary>
        /// Returns a status code the given archive. Requesting status for a given archive will also start
        /// generating that archive.
        /// 0 : Archive creation started.
        ///  1 : Archive is being created.
        ///  2 : Archive is available for download.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{packageId}/status")]
        public ActionResult<int> GetArchiveStatus(string packageId)
        {
            try
            {
                IndexService.PurgeOldArchives();

                return IndexService.GetPackageArchiveStatus(packageId);
            }
            catch (PackageNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                Console.WriteLine(ex);
                return Responses.UnexpectedError();
            }
        }
    }
}
