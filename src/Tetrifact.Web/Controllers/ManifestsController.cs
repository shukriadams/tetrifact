using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ManifestsController : Controller
    {
        #region FIELDS

        private readonly ITetriSettings _settings;
        private IIndexReader _indexService;
        private ILogger<ManifestsController> _log;

        #endregion

        #region CTORS

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
            _indexService = indexService;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{packageId}")]
        public ActionResult<Manifest> GetManifest(string packageId)
        {
            return _indexService.GetManifest(packageId);
        }

        #endregion
    }
}
