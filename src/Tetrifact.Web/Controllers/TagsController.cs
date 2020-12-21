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
        [HttpGet("")]
        public ActionResult<string[]> GetTags()
        {
            try
            {
                return _tagsService.GetAllTags().ToArray();
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


        /// <summary>
        /// GEts a list of package ids which have the given tags.
        /// </summary>
        /// <param name="tags">Comma-separated list of tags.</param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{tag}/packages")]
        public ActionResult<string[]> GetTagPackages(string tags)
        {
            try
            {
                string[] tagsSplit = tags.Split(",", StringSplitOptions.RemoveEmptyEntries);
                return _tagsService.GetPackageIdsWithTags(tagsSplit).ToArray();
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
        [HttpPost("{tag}/{packageId}")]
        public ActionResult AddTag(string tag, string packageId)
        {
            try
            {
                tag = HttpUtility.UrlDecode(tag);
                _tagsService.AddTag(packageId, tag);
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
        [HttpDelete("{tag}/{packageId}")]
        public ActionResult RemoveTag(string tag, string packageId)
        {
            try
            {
                tag = HttpUtility.UrlDecode(tag);
                _tagsService.RemoveTag(packageId, tag);
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
