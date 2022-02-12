using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class CleanController : Controller
    {
        #region FIELDS

        private readonly IIndexReadService _indexService;

        private readonly IArchiveService _archiveService;

        private readonly IRepositoryCleanService _repositoryCleaner;

        private readonly ILogger<CleanController> _log;
        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public CleanController(IRepositoryCleanService repositoryCleaner, IIndexReadService indexService, IArchiveService archiveService, ILogger<CleanController> log)
        {
            _indexService = indexService;
            _repositoryCleaner = repositoryCleaner;
            _archiveService = archiveService;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Force runs clean on deleted packages, old archives etc.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpGet("")]
        public ActionResult Clean()
        {
            try 
            {
                _repositoryCleaner.Clean();
                _archiveService.PurgeOldArchives();

                return new JsonResult(new
                {
                    success = new
                    {
                        description = "Clean complete"
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError(ex.Message);
            }
        }

        #endregion
    }
}
