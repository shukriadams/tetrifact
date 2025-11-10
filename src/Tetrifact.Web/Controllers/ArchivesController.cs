using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ArchivesController : Controller
    {
        #region FIELDS

        private readonly IArchiveService _archiveService;

        private readonly IIndexReadService _indexReader;

        private readonly ILogger<ArchivesController> _log;

        private readonly IFileSystem _fileSystem;

        private readonly IProcessManager _ticketManager;

        private readonly IProcessManager _activeDownloadsTracker;

        private readonly IQueueHandler _queueHandler;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public ArchivesController(IArchiveService archiveService, IQueueHandler queueHandler, IProcessManagerFactory processManagerFactory, IFileSystem fileSystem, IIndexReadService indexReader, ILogger<ArchivesController> log)
        {
            _queueHandler = queueHandler;
            _archiveService = archiveService;
            _indexReader = indexReader;
            _fileSystem = fileSystem;
            _ticketManager = processManagerFactory.GetInstance(ProcessManagerContext.ArchiveTickets);
            _activeDownloadsTracker = processManagerFactory.GetInstance(ProcessManagerContext.ArchiveActiveDownloads);
            _log = log;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{packageId}/queue")]
        public ActionResult QueueArchiveGeneration(string packageId)
        {
            try
            {
                _archiveService.QueueArchiveCreation(packageId);
                return Redirect($"/package/{packageId}");
            }
            catch (PackageNotFoundException ex)
            {
                _log.LogInformation($"{ex}");
                return Responses.NotFoundError(this, packageId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Downloads an archive, starts its creation if archive doesn't exist. Returns when archive is available. 
        /// Supports range processing for resuming broken downloads.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{packageId}")]
        public ActionResult GetArchive(string packageId, [FromQuery(Name = "ticket")] string ticket, [FromQuery(Name = "waiver")] string waiver)
        {
            // note ticket and waiver are never empty, controller enforces that
            try
            {
                // if not exists, queue and redirect back to view
                if (!_indexReader.PackageExists(packageId))
                    throw new PackageNotFoundException(packageId);

                RequestHeaders headers = Request.GetTypedHeaders();

                // capture optional range request - this is needed if client wants to download
                // a segment of an archive
                string range = string.Empty;
                if (Request.Headers.Range.Count != 0) 
                    range = Request.Headers.Range;

                if (Request.Headers.ContentRange.Count != 0)
                    range = Request.Headers.ContentRange;

                // note : ip is required if queueing is enabled
                string ip = string.Empty;
                if (Request.HttpContext.Connection.RemoteIpAddress != null)
                    ip = Request.HttpContext.Connection.RemoteIpAddress.ToString().ToLower();

                // enforce ticket if queue enabled
                string ticketLog = string.Empty;
                QueueResponse queueResponse = _queueHandler.ProcessRequest(ip, ticket, waiver);
                if (queueResponse.Status == QueueStatus.Deny) 
                {
                    _log.LogInformation($"archive request rejected for ip {ip} with ticket {ticket}, reason : {queueResponse.Reason}.");
                    return Responses.NoTicket();
                }

                if (queueResponse.Status == QueueStatus.Wait)
                {
                    _log.LogInformation($"Queued ip {ip} with ticket {ticket} forced to wait, position {queueResponse.WaitPosition}.");
                    return Responses.QueueFull(queueResponse.WaitPosition, queueResponse.WaitPosition); // refactor this
                }

                string archivePath = _archiveService.GetPackageArchivePath(packageId);
                int totalTicketCount = _ticketManager.GetAll().Count();
                if (!_fileSystem.File.Exists(archivePath))
                {
                    _archiveService.QueueArchiveCreation(packageId);
                    return Redirect($"/package/{packageId}");
                }

                if (!string.IsNullOrEmpty(range))
                    range = $" range {range},";
                else
                    range = " no range";


                Stream archiveStream = _archiveService.GetPackageAsArchive(packageId);
                ProgressableStream progressableStream = new ProgressableStream(archiveStream);

                progressableStream.OnComplete =()=>{
                    _activeDownloadsTracker.RemoveUnique(ticket);
                    // don't delete ticket, let it time out passively
                };

                Debounce keepAlive = new Debounce(new TimeSpan(0, 0, 1), () => {
                    // keep ticket + tracker alive for duration of download
                    _ticketManager.KeepAlive(ticket, $"Range:{range}, package:{packageId}");
                    _activeDownloadsTracker.KeepAlive(ticket, string.Empty);
                });

                progressableStream.OnProgress = (long progress, long total) => {
                    keepAlive.Invoke();
                };

                _activeDownloadsTracker.AddUnique(ticket, new TimeSpan(0, 10, 0), $"IP:{ip}, package:{packageId}, range:{range}, waiver:{waiver}");
                _log.LogInformation($"Serving archive for package \"{packageId}\" to IP:\"{ip}\" range:\"{range}\" ticket:\"{ticket}\" {ticketLog}, queue reason:{queueResponse.Reason}, queue size {totalTicketCount}, active download count is {_activeDownloadsTracker.Count()}.");

                return File(
                    progressableStream, 
                    "application/octet-stream", 
                    $"{packageId}.zip", 
                    enableRangeProcessing: true);
            }
            catch (PackageNotFoundException ex)
            {
                _log.LogInformation($"{ex}");
                return Responses.NotFoundError(this, packageId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Returns JSON with status code the given archive.
        /// 0 : Archive does not exist and has not been queued for creation.
        /// 1 : Archive is being created.
        /// 2 : Archive is available for download.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{packageId}/status")]
        public ActionResult GetArchiveStatus(string packageId)
        {
            try
            {
                return new JsonResult(new
                {
                    success = new
                    {
                        status = _archiveService.GetPackageArchiveStatus(packageId)
                    }
                });
            }
            catch (PackageNotFoundException ex)
            {
                _log.LogInformation($"{ex}");
                return Responses.NotFoundError(this, packageId);
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
