using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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

                string ticket = Guid.NewGuid().ToString();
                _processManager.AddUnique(ProcessCategories.ArchiveQueueSlot, ticket, requestIdentifier, new TimeSpan(0, 0, _settings.DownloadQueueTicketLifespan));

                return new JsonResult(new
                {
                    success = new
                    {
                        ticket = ticket,
                        clientIdentifier = requestIdentifier,
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
