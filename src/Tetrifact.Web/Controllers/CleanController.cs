using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;

namespace Tetrifact.Web.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class CleanController : Controller
    {
        #region FIELDS

        private readonly ITetriSettings _settings;
        private IIndexReader _indexService;
        private ILogger<CleanController> _log;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public CleanController(ITetriSettings settings, IIndexReader indexService, ILogger<CleanController> log)
        {
            _settings = settings;
            _indexService = indexService;
            _log = log;
        }

        #endregion

        #region METHODS

        [HttpGet("")]
        public ActionResult Clean()
        {
            _indexService.CleanRepository();
            _indexService.PurgeOldArchives();
            return Ok("Clean complete");
        }

        #endregion
    }
}
