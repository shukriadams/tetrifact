using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
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

        private readonly IMemoryCache _cache;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public ArchivesController(IArchiveService archiveService, IMemoryCache cache, IQueueHandler queueHandler, IProcessManagerFactory processManagerFactory, IFileSystem fileSystem, IIndexReadService indexReader, ILogger<ArchivesController> log)
        {
            _cache = cache;
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
        public ActionResult GetArchive(string packageId)
        {
            // note ticket and waiver are never empty, controller enforces that
            try
            {
                // if not exists, queue and redirect back to view
                if (!_indexReader.PackageExists(packageId))
                    throw new PackageNotFoundException(packageId);

                string archivePath = _archiveService.GetPackageArchivePath(packageId);
                bool creatingArchive = false;
                if (!_fileSystem.File.Exists(archivePath))
                {
                    _archiveService.QueueArchiveCreation(packageId);
                    creatingArchive = true;
                }

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
                QueueResponse queueResponse = _queueHandler.ProcessRequest(ip);
                if (queueResponse.Status == QueueStatus.Deny)
                {
                    _log.LogInformation($"archive request rejected for ip {ip}, reason : {queueResponse.Reason}.");
                    return Responses.NoTicket();
                }

                if (queueResponse.Status == QueueStatus.Wait)
                {
                    _log.LogInformation($"Queued ip {ip} forced to wait, position {queueResponse.QueueLength}.");
                    return Responses.QueueFull(queueResponse.QueueLength); // refactor this
                }

                int totalTicketCount = _ticketManager.GetAll().Count();

                if (!string.IsNullOrEmpty(range))
                    range = $" range {range},";
                else
                    range = " no range";

                if (creatingArchive && queueResponse.IsLocal) 
                {
                    return Redirect($"/package/{packageId}");
                }

                Regex regex = new Regex("range bytes=\\d*-(\\d*)");
                Match match = regex.Match(range);
                long? rangeEnd = null;
                if (match.Success) 
                {
                    rangeEnd = long.Parse(match.Groups[1].Value);
                }

                Stream archiveStream = _archiveService.GetPackageAsArchive(packageId);
                ProgressableStream progressableStream = rangeEnd.HasValue ?
                    new ProgressableStream(archiveStream, rangeEnd.Value) :
                    new ProgressableStream(archiveStream);

                progressableStream.OnRangeComplete =()=>{
                    // once user has streamed a requested section of an archive, allow them to download another section, keeping their position in queue
                    _activeDownloadsTracker.RemoveUnique(ip);
                };

                progressableStream.OnResourceComplelete = () => {
                    // once user has streamed the archive in its entirety, yoink their queue ticket
                    _ticketManager.RemoveUnique(ip);
                };

                Debounce keepAlive = new Debounce(new TimeSpan(0, 0, 1), () => {
                    // keep ticket + tracker alive for duration of download
                    _ticketManager.KeepAlive(ip, $"Range:{range}, package:{packageId}");
                    _activeDownloadsTracker.KeepAlive(ip, string.Empty);
                });

                progressableStream.OnProgress = (long progress, long total) => {
                    keepAlive.Invoke();
                };

                _activeDownloadsTracker.AddUnique(ip, new TimeSpan(0, 10, 0), $"IP:{ip}, package:{packageId}, range:{range}");
                _log.LogInformation($"Serving archive for package \"{packageId}\" to IP:\"{ip}\" range:\"{range}\" {ticketLog}, queue reason:{queueResponse.Reason}, queue size {totalTicketCount}, active download count is {_activeDownloadsTracker.Count()}.");

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
        /// 
        /// If the archive is genereated, returns the length of the archive stream. Returns null 
        /// if archive is not generated.
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
                ArchiveProgressInfo status = _archiveService.GetPackageArchiveStatus(packageId);

                long? streamLength = null;
                
                if (status.State == PackageArchiveCreationStates.Processed_ArchiveAvailable) 
                {
                    // note : this cache will return incorrect results if a package is deleted
                    // and recreated. The solution is to move to a unique package id system instead
                    // of one the user defines.
                    string key = $"streamLength_{packageId}";

                    streamLength = _cache.Get(key) as long?;
                    if (!streamLength.HasValue) 
                    {
                        Stream archiveStream = _archiveService.GetPackageAsArchive(packageId);
                        streamLength = archiveStream.Length;
                        _cache.Set(key, streamLength);
                    }
                }
                
                return new JsonResult(new
                {
                    success = new
                    {
                        status,
                        length = streamLength
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
