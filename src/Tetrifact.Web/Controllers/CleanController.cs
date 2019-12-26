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

        private readonly ICleaner _repositoryCleaner;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public CleanController(ICleaner repositoryCleaner)
        {
            _repositoryCleaner = repositoryCleaner;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(WriteLevel))]
        [HttpGet("{project}")]
        public ActionResult Clean(string project)
        {
            _repositoryCleaner.Clean(project);
            return Ok("Clean complete");
        }

        #endregion
    }
}
