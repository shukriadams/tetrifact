using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class CleanController : Controller
    {
        #region FIELDS

        private readonly IIndexReader _indexService;
        private readonly IRepositoryCleaner _repositoryCleaner;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public CleanController(IRepositoryCleaner repositoryCleaner, IIndexReader indexService)
        {
            _indexService = indexService;
            _repositoryCleaner = repositoryCleaner;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(WriteLevel))]
        [HttpGet("{project}")]
        public ActionResult Clean(string project)
        {
            _repositoryCleaner.Clean(project);
            _indexService.PurgeOldArchives();
            return Ok("Clean complete");
        }

        #endregion
    }
}
