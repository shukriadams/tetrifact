using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
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

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public ArchivesController(IArchiveService archiveService, IFileSystem fileSystem, IIndexReadService indexReader, ILogger<ArchivesController> log)
        {
            _archiveService = archiveService;
            _indexReader = indexReader;
            _fileSystem = fileSystem;
            _log = log;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{packageId}/queue")]
        public ActionResult QueueArchiveGeneration(string packageId)
        {
            try
            {
                _archiveService.QueueArchiveCreation(packageId);

                return Redirect($"/package/{packageId}");

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

        /// <summary>
        /// Gets an archive, starts its creation if archive doesn't exist. Returns when archive is available. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{packageId}")]
        public ActionResult GetArchive(string packageId)
        {
            try
            {
                // if not exists, queue and redirect back to view
                if (!_indexReader.PackageExists(packageId))
                    throw new PackageNotFoundException(packageId);

                string archivePath = _archiveService.GetPackageArchivePath(packageId);
                if (!_fileSystem.File.Exists(archivePath))
                {
                    _archiveService.QueueArchiveCreation(packageId);
                    return Redirect($"/package/{packageId}");
                }

                Stream archiveStream = _archiveService.GetPackageAsArchive(packageId);
                _log.LogInformation($"served archived for package \"{packageId}\".");

                return File(archiveStream, "application/octet-stream", $"{packageId}.zip", enableRangeProcessing: true);
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
