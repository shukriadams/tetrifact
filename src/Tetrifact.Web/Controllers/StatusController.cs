using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class StatusController : Controller
    {
        #region FIELDS

        private readonly IDiffServiceProvider _diffServiceProvider;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public StatusController(IDiffServiceProvider diffServiceProvider)
        {
            _diffServiceProvider = diffServiceProvider;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("diffservice")]
        public ActionResult DiffService()
        {
            if (_diffServiceProvider.Instance.LastRun == null)
                return Problem("Diff Service is not running");
                        
            return Ok($"last run : {_diffServiceProvider.Instance.LastRun.Value} ({ (DateTime.UtcNow - _diffServiceProvider.Instance.LastRun.Value).TotalSeconds} seconds ago)");
        }

        #endregion
    }
}
