using Microsoft.AspNetCore.Mvc;
using System;
using Tetrifact.Core;
using Tetrifact.Core.Porter_Packages.Madscience.Loggger;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class MetricsController : Controller
    {
        #region FIELDS

        private readonly IMetricsService _metricsService;

        private readonly ILoggger _log;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagsService"></param>
        /// <param name="log"></param>
        public MetricsController(
            IMetricsService metricsService, 
            ILoggger log)
        {
            _metricsService = metricsService;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Gets metrics for server in InfluxDB format
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("influx")]
        public ActionResult<string> Influx()
        {
            try
            {
                return _metricsService.GetInfluxMetrics();
            }
            catch (MetricsStaleException ex) 
            {
                _log.Error(this, "Failed to get current influx metrics.", ex);
                return Responses.UnexpectedError($"Metrics retrievail failed : {ex}. You can check logs for additional information.");
            }
            catch (Exception ex)
            {
                _log.Error(this, ex);
                return Responses.UnexpectedError();
            }
        }

        #endregion
    }
}
