﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
                string range = string.Empty;
                if (Request.Headers.Range.Count != 0) 
                    range = Request.Headers.Range;

                if (Request.Headers.ContentRange.Count != 0)
                    range = Request.Headers.ContentRange;

                string ip = string.Empty;
                if (Request.HttpContext.Connection.RemoteIpAddress != null)
                    ip = Request.HttpContext.Connection.RemoteIpAddress.ToString().ToLower();

                bool isLocal = _settings.WhiteListedLocalAddresses.Contains(ip.ToLower());
                string ticketLog = string.Empty;

                // local (this website) downloads always allowed.
                if (!isLocal && _settings.MaximumSimultaneousDownloads.HasValue)
                {
                    if (string.IsNullOrEmpty(ticket))
                    {
                        _log.LogInformation($"Rejected download request because of missing ticket, origin is \"{ip}\".");
                        return Responses.NoTicket();
                    }

                    // check for priority tickets
                    if (_settings.DownloadQueuePriorityTickets.Contains(ticket))
                    {
                        _log.LogInformation($"Priority ticket {ticket} used from host \"{ip}\".");
                        ticketLog = $" priority ticket {ticket},";
                    }
                    else
                    {
                        IEnumerable<ProcessItem> userTickets = _processManager.GetByCategory(ProcessCategories.ArchiveQueueSlot);
                        ProcessItem userTicket = userTickets.FirstOrDefault(i => i.Id == ticket);
                        if (userTicket == null)
                            return Responses.NoTicket();

                        int count = userTickets.Where(t => t.AddedUTC < userTicket.AddedUTC).Count();
                        if (count > _settings.MaximumSimultaneousDownloads)
                            return Responses.QueueFull(count, _settings.MaximumSimultaneousDownloads.Value);

                        ticketLog = $" dynamic ticket {ticket},";
                    }
                }
                else 
                {
                    ticketLog = $" ticket not enforced, received:{ticket},";
                }

                string archivePath = _archiveService.GetPackageArchivePath(packageId);
                if (!_fileSystem.File.Exists(archivePath))
                {
                    _archiveService.QueueArchiveCreation(packageId);
                    return Redirect($"/package/{packageId}");
                }

                if (!string.IsNullOrEmpty(range))
                    range = $" range {range},";
                else
                    range = " no range";

                _log.LogInformation($"Serving archive for package {packageId} to {ip}{range},{ticketLog}");

                Stream archiveStream = _archiveService.GetPackageAsArchive(packageId);
                ProgressableStream progressableStream = new ProgressableStream(archiveStream);
                progressableStream.OnComplete = ()=>{
                    // clear ticket once all package has been streamed
                    if (!string.IsNullOrEmpty(ticket))
                        _processManager.RemoveUnique(ticket);
                };

                progressableStream.OnProgress = (long progress, long total) => {
                    // keep ticket alive for duration of download
                    if (!string.IsNullOrEmpty(ticket))
                        _processManager.KeepAlive(ticket, $"Range:{range}, package:{packageId}");
                };

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
