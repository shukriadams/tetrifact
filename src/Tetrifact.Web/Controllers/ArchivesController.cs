﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ArchivesController : Controller
    {
        #region FIELDS

        private readonly IIndexReader _indexService;
        private readonly ILogger<ArchivesController> _log;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public ArchivesController(IIndexReader indexService, ILogger<ArchivesController> log)
        {
            _indexService = indexService;
            _log = log;
        }

        #endregion

        #region METHODS


        /// <summary>
        /// Gets an archive, starts its creation if archive doesn't exist. Returns when archive is available. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{packageId}")]
        public ActionResult GetArchive(string packageId)
        {
            Stream archiveStream = null;

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                _indexService.PurgeOldArchives();
                archiveStream = _indexService.GetPackageAsArchive(packageId);

                sw.Stop();
                _log.LogInformation($"Archive generation for package {packageId} took {0} seconds", sw.Elapsed.TotalSeconds);

                return File(archiveStream, "application/octet-stream", $"{packageId}.zip");
            }
            catch (PackageNotFoundException)
            {
                return Responses.NotFoundError(this, $"Package ${packageId} not found.");
            }
            catch (Exception ex)
            {
                if (archiveStream != null)
                    archiveStream.Close();

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
                _indexService.PurgeOldArchives();

                return new JsonResult(new
                {
                    success = new
                    {
                        status = _indexService.GetPackageArchiveStatus(packageId)
                    }
                });
            }
            catch (PackageNotFoundException)
            {
                return Responses.NotFoundError(this, $"Package ${packageId} not found.");
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
