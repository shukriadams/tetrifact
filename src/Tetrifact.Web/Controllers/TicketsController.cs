using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class TicketsController : Controller
    {
        #region FIELDS

        private readonly ILogger<ArchivesController> _log;

        private readonly IProcessManager _processManager;

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
        public TicketsController(IArchiveService archiveService, ISettings settings, IProcessManager processManager, ILogger<ArchivesController> log)
        {
            _settings = settings;
            _processManager = processManager;
            _log = log;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(WriteLevel))]
        [HttpPost("{requestIdentifier}")]
        public ActionResult CreateQueueTicket(string requestIdentifier)
        {
            try
            {
                // skip ticket creation if there is no queue limit
                if (!_settings.MaximumSimultaneousDownloads.HasValue)
                    return new JsonResult(new
                    {
                        success = new
                        {
                            ticket = string.Empty,
                            message= "Ticket not required, none generated",
                            required = false
                        }
                    });

                if (Request.HttpContext.Connection.RemoteIpAddress == null)
                    return new JsonResult(new
                    {
                        error = new
                        {
                            message = "Failed to resolve IP",
                            required = false
                        }
                    });

                string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                string ticket = Obfuscator.Cloak(ip);
                string requestLoggedIdentifier = $"ReqID:{requestIdentifier},IP:{ip}";

                ProcessCreateResponse response = _processManager.AddRestrained(
                    ProcessCategories.ArchiveQueueSlot,
                    ticket,
                    requestLoggedIdentifier);

                if (!response.Success)
                    return new JsonResult(new
                    {
                        error = new
                        {
                            message = response.Message
                        }
                    });

                return new JsonResult(new
                {
                    success = new
                    {
                        ticket,
                        required = true
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }

        #endregion
    }
}
