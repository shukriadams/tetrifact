using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class MetricsController : Controller
    {
        #region FIELDS

        private readonly IMetricsService _metricsService;
        private readonly ILogger<MetricsController> _log;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagsService"></param>
        /// <param name="log"></param>
        public MetricsController(IMetricsService metricsService, ILogger<MetricsController> log)
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
                _log.LogError(ex, "Failed to get current influx metrics.");
                return Responses.UnexpectedError($"Metrics retrievail failed : {ex}. You can check logs for additional information.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }

        #endregion
    }
}
