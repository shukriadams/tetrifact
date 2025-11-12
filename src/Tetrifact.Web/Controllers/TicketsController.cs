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

        private readonly IProcessManager _ticketManager;

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
        public TicketsController(ISettings settings, IProcessManagerFactory processManagerFactory, ILogger<ArchivesController> log)
        {
            _settings = settings;
            _ticketManager = processManagerFactory.GetInstance(ProcessManagerContext.ArchiveTickets);
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
                            description = "Ticket not required, none generated",
                            required = false
                        }
                    }) { StatusCode = 200 };

                if (Request.HttpContext.Connection.RemoteIpAddress == null)
                    return new JsonResult(new
                    {
                        error = new
                        {
                            description = "Failed to resolve IP"
                        }
                    }) { StatusCode = 500 };

                string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                string ticket = Guid.NewGuid().ToString();
                string requestLoggedIdentifier = $"ReqID:{requestIdentifier},IP:{ip}";

                _ticketManager.AddUnique(
                    ticket,
                    new TimeSpan(0, 0, _settings.DownloadQueueTicketLifespan),
                    requestLoggedIdentifier);

                return new JsonResult(new
                {
                    success = new
                    {
                        ticket,
                        required = true
                    }
                }) { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(WriteLevel))]
        [HttpDelete("{ticket}")]
        public ActionResult Delete(string ticket)
        {
            try 
            {
                if (!_ticketManager.HasKey(ticket))
                    return new JsonResult(new
                    {
                        error = new {
                            description = $"Ticket {ticket} does not exist"
                        }
                    }) { StatusCode = 404 };

                _ticketManager.RemoveUnique(ticket);

                return new JsonResult(new
                {
                    success = new {
                        description = "Ticket removed"
                    }
                })
                { StatusCode = 200 };

            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unexpected error removing ticket {ticket}");
            }

            return new JsonResult(new
            {
                error = new {
                    description = "Ticket removal failed. Check logs."
                }
            })
            { StatusCode = 500 };

        }

        #endregion
    }
}
