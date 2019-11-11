using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Web;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class TagsController : Controller
    {
        #region FIELDS

        private readonly ITagsService _tagsService;
        private readonly ILogger<TagsController> _log;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagsService"></param>
        /// <param name="log"></param>
        public TagsController(ITagsService tagsService, ILogger<TagsController> log)
        {
            _tagsService = tagsService;
            _log = log;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{project}")]
        public ActionResult<string[]> GetTags(string project)
        {
            try
            {
                return _tagsService.GetAllTags(project).ToArray();
            }
            catch (PackageNotFoundException)
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


        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{project}/{tag}/packages")]
        public ActionResult<string[]> GetTagPackages(string project, string tag)
        {
            try
            {
                return _tagsService.GetPackagesWithTag(project, tag).ToArray();
            }
            catch (PackageNotFoundException)
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


        [ServiceFilter(typeof(WriteLevel))]
        [HttpPost("{project}/{tag}/{packageId}")]
        public ActionResult AddTag(string project, string tag, string packageId)
        {
            try
            {
                tag = HttpUtility.UrlDecode(tag);
                _tagsService.AddTag(project, packageId, tag);
                return Ok($"Tag {tag} was added to package {packageId}");
            }
            catch (PackageNotFoundException)
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


        [ServiceFilter(typeof(WriteLevel))]
        [HttpDelete("{project}/{tag}/{packageId}")]
        public ActionResult RemoveTag(string project, string tag, string packageId)
        {
            try
            {
                tag = HttpUtility.UrlDecode(tag);
                _tagsService.RemoveTag(project, packageId, tag);
                return Ok($"Tag {tag} was removed from package {packageId}");
            }
            catch (PackageNotFoundException)
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
