using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;

namespace Tetrifact.Web.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class CleanController : Controller
    {
        private readonly ITetriSettings _settings;
        public IIndexReader IndexService;
        private ILogger<CleanController> _log;

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
            IndexService = indexService;
            _log = log;
        }

        [HttpGet("")]
        public ActionResult Clean()
        {
            this.IndexService.Clean();
            this.IndexService.PurgeOldArchives();
            return Ok("Clean complete");
        }
    }
}
