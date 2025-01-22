using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
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
        public ArchivesController(IArchiveService archiveService, ISettings settings, IProcessManager processManager, IFileSystem fileSystem, IIndexReadService indexReader, ILogger<ArchivesController> log)
        {
            _archiveService = archiveService;
            _indexReader = indexReader;
            _fileSystem = fileSystem;
            _settings = settings;
            _processManager = processManager;
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

        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(WriteLevel))]
        public ActionResult GetQueueTicket(string requestIdentifier) 
        {
            try
            {
                if (!_settings.MaximumSimultaneousDownloads.HasValue)
                    return new JsonResult(new
                    {
                        success = new
                        {
                            ticket = string.Empty,
                            required = false
                        }
                    });

                if (_processManager.GetByCategory(ProcessCategories.ArchiveQueueSlot).Count() >= _settings.MaximumSimultaneousDownloads)
                    return new JsonResult(new
                    {
                        error = new
                        {
                            code = 1,
                            message = $"Queue is full ({_settings.MaximumSimultaneousDownloads}), please try again later"
                        }
                    });

                string ticket = Guid.NewGuid().ToString();
                _processManager.AddUnique(ProcessCategories.ArchiveQueueSlot, ticket, requestIdentifier, new TimeSpan(0, 0, _settings.DownloadQueueTicketLifespan));
                return new JsonResult(new
                {
                    success = new
                    {
                        ticket = ticket,
                        required = true
                    }
                }); ;
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
        public ActionResult GetArchive(string packageId, [FromQuery(Name = "ticket")] string ticket)
        {
            try
            {
                // if not exists, queue and redirect back to view
                if (!_indexReader.PackageExists(packageId))
                    throw new PackageNotFoundException(packageId);

                // enforce ticket if queue enabled
                RequestHeaders headers = Request.GetTypedHeaders();
                Uri referer = headers.Referer;
                bool isLocal = referer.Host.ToLower() == "localhost";

                if (!isLocal && _settings.MaximumSimultaneousDownloads.HasValue) 
                {
                    ProcessItem item = _processManager.GetByCategory(ProcessCategories.ArchiveQueueSlot).FirstOrDefault(i => i.Id == ticket);
                    if (item == null)
                        return Responses.QueueFull();
                }

                string archivePath = _archiveService.GetPackageArchivePath(packageId);
                if (!_fileSystem.File.Exists(archivePath))
                {
                    _archiveService.QueueArchiveCreation(packageId);
                    return Redirect($"/package/{packageId}");
                }

                _log.LogInformation($"Serving archive for package \"{packageId}\".");

                Stream archiveStream = _archiveService.GetPackageAsArchive(packageId);
                ProgressableStream progressableStream = new ProgressableStream(archiveStream);
                progressableStream.OnComplete = ()=>{
                    // clear ticket once all package has been streamed
                    if (!isLocal && !string.IsNullOrEmpty(ticket))
                        _processManager.RemoveUnique(ticket);
                };

                return File(progressableStream, "application/octet-stream", $"{packageId}.zip", enableRangeProcessing: true);
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
