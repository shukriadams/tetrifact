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
        public TicketsController(ISettings settings, IProcessManager processManager, ILogger<ArchivesController> log)
        {
            _settings = settings;
            _processManager = processManager;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Creates a download ticket. Every request gets a ticket, which is basically just a way to reference when a user has askes to be able 
        /// to download something. The order in which tickets are serviced is up to the controller servicing the ticket later.
        /// </summary>
        /// <param name="requestIdentifier"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(WriteLevel))]
        [HttpPost("{requestIdentifier}")]
        public ActionResult Add(string requestIdentifier)
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
                    }) { StatusCode = 200 } ;

                if (Request.HttpContext.Connection.RemoteIpAddress == null)
                    return new JsonResult(new
                    {
                        error = new
                        {
                            message = "Failed to resolve IP"
                        }
                    }) { StatusCode = 500 };

                string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                string ticket = Guid.NewGuid().ToString();
                string requestLoggedIdentifier = $"ReqID:{requestIdentifier},IP:{ip}";

                ProcessCreateResponse response = _processManager.AddByCategory(
                    ProcessCategories.ArchiveQueueSlot,
                    new TimeSpan(0, 0, _settings.DownloadQueueTicketLifespan),
                    ticket,
                    requestLoggedIdentifier);

                if (!response.Success)
                    return new JsonResult(new
                    {
                        error = new
                        {
                            message = response.Message
                        }
                    }){ StatusCode = 500 };

                return new JsonResult(new
                {
                    success = new
                    {
                        ticket,
                        required = true
                    }
                }){ StatusCode = 200 };
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
