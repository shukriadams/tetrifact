using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class CleanController : Controller
    {
        #region FIELDS

        private readonly IArchiveService _archiveService;

        private readonly IRepositoryCleanServiceFactory _serviceFactory;
        
        private readonly ILogger<CleanController> _log;

        private readonly ISettings _settings;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public CleanController(IRepositoryCleanServiceFactory serviceFactory, ISettings settings, IArchiveService archiveService, ILogger<CleanController> log)
        {
            _serviceFactory = serviceFactory;
            _archiveService = archiveService;
            _settings = settings;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Force runs clean on deleted packages, old archives etc.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(WriteLevel))]
        [HttpGet("")]
        public ActionResult Clean()
        {
            try
            {
                if (!_settings.EnableCleanViaController)
                    return Responses.NoPermission();
                
                _log.LogInformation("Starting clean from controller");
                IRepositoryCleanService repositoryCleaner = _serviceFactory.Create();
                CleanResult cleaned = repositoryCleaner.Clean();
                _archiveService.PurgeOldArchives();

                return new JsonResult(new
                {
                    success = new
                    {
                        cleaned = cleaned.Cleaned,
                        failed = cleaned.Failed,
                        directoriesScanned = cleaned.DirectoriesScanned,
                        filesScanned = cleaned.FilesScanned,
                        packagesInSystem = cleaned.PackagesInSystem,
                        description = cleaned.Description 
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError(ex.Message);
            }
        }

        #endregion
    }
}
