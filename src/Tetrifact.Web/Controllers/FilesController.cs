using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class FilesController : Controller
    {
        #region FIELDS

        private readonly IIndexReadService _indexService;
        
        private readonly ILogger<FilesController> _log;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public FilesController(IIndexReadService indexService, ILogger<FilesController> log)
        {
            _indexService = indexService;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Downloads a package file.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{fileId}")]
        public ActionResult GetItem(string fileId)
        {
            try
            {
                GetFileResponse payload = _indexService.GetFile(fileId);
                if (payload == null)
                    return Responses.NotFoundError(this, $"file {fileId} not found. Id is invalid, or has been deleted.");

                if (payload.Content == null)
                    throw new Exception($"File {fileId} has no content, possible data corruption.");

                return File(payload.Content, "application/octet-stream", payload.FileName, enableRangeProcessing: true);
            }
            catch (InvalidFileIdentifierException)
            {
                return Responses.InvalidFileId();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }

        #endregion
    }
}
