using System;
using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using Tetrifact.Core.Porter_Packages.Madscience.Loggger;
using Tetrifact.Core.Porter_Packages.Madscience.Loggger;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class PruneController : Controller
    {
        #region FIELDS
        
        private readonly IPruneServiceFactory _serviceFactory;

        private readonly ILoggger _log;

        private readonly ISettings _settings;
        
        #endregion
        
        #region CTORS
        
        public PruneController(
            IPruneServiceFactory serviceFactory, 
            ISettings settings, 
            ILoggger log)
        {
            _serviceFactory = serviceFactory;
            _log = log;
            _settings = settings;
        }

        #endregion
        
        #region METHODS
        
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
                
                _log.Status(this, "Starting clean from controller");
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
                _log.Error(this, ex);
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
        
        #endregion
    }
}
