using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ArchivesController : Controller
    {
        #region FIELDS

        private readonly IIndexReader _indexReader;
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
            _indexReader = indexService;
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
        [HttpGet("{project}/{package}")]
        public ActionResult GetArchive(string project, string package)
        {
            try
            {
                Stream archiveStream = _indexReader.GetPackageAsArchive(project, package);
                return File(archiveStream, "application/octet-stream", $"{package}.zip");
            }
            catch (PackageNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                Console.WriteLine(ex);
                return Responses.UnexpectedError();
            }
        }

        #endregion
    }
}
