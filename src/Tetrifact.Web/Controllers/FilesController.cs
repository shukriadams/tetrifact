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

        private readonly IIndexReader _indexService;
        
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
        public FilesController(IIndexReader indexService, ILogger<FilesController> log)
        {
            _indexService = indexService;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{project}/{fileId}")]
        public ActionResult GetItem(string project, string fileId)
        {
            try
            {
                GetFileResponse payload = _indexService.GetFile(project, fileId);
                return File(payload.Content, "application/octet-stream", payload.FileName);
            }
            catch (InvalidFileIdentifierException)
            {
                return Responses.InvalidFileId();
            }
            catch (PackageNotFoundException)
            {
                return NotFound();
            }
            catch (Tetrifact.Core.FileNotFoundException) 
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                Console.WriteLine("An unexpected error occurred : ");
                Console.WriteLine(ex);
                return Responses.UnexpectedError();
            }
            
        }

        #endregion
    }
}
