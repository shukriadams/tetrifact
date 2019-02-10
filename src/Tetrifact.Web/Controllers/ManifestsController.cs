using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ManifestsController : Controller
    {
        private readonly ITetriSettings _settings;
        public IIndexReader IndexService;
        private ILogger<ManifestsController> _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public ManifestsController(ITetriSettings settings, IIndexReader indexService, ILogger<ManifestsController> log)
        {
            _settings = settings;
            IndexService = indexService;
            _log = log;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{packageId}")]
        public ActionResult<Manifest> GetManifest(string packageId)
        {
            return IndexService.GetManifest(packageId);
        }
    }
}
