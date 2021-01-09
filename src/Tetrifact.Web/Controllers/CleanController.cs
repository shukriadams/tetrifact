using Microsoft.AspNetCore.Mvc;
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

        /// <summary>
        /// Force runs clean on deleted packages, old archives etc.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpGet("")]
        public ActionResult Clean()
        {
            _repositoryCleaner.Clean();
            _indexService.PurgeOldArchives();
            return new JsonResult(new
            {
                success = new
                {
                    description = "Clean complete"
                }
            });
        }

        #endregion
    }
}
