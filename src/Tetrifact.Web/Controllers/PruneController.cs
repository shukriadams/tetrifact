using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class PruneController
    {
        private readonly IPruneServiceFactory _serviceFactory;

        private readonly ILogger<PruneController> _log;

        private readonly ISettings _settings;
            
        public PruneController(IPruneServiceFactory serviceFactory, ISettings settings, ILogger<PruneController> log)
        {
            _serviceFactory = serviceFactory;
            _log = log;
            _settings = settings;
        }

        /// <summary>
        /// Forces a prune.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(WriteLevel))]
        [HttpGet("")]
        public ActionResult Prune()
        {
            try 
            {
                if (!_settings.EnablePruneViaController)
                    return Responses.NoPermission();
                
                _log.LogInformation("Starting clean from controller");
                IPruneService pruneService = _serviceFactory.Create();
                PrunePlan prunePlan = pruneService.Prune();

                return new JsonResult(new
                {
                    success = new
                    {
                        prunePlan 
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError(ex.Message);
            }
        }
        
        [Route("report")]
        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(ReadLevel))]
        public string Report()
        {
            IPruneService pruneService = _serviceFactory.Create();
            PrunePlan report = pruneService.GeneratePrunePlan();
            string s = string.Empty;
            foreach(string l in report.Report)
                s += l+ "\n";

            return s;
        }
    }
}
