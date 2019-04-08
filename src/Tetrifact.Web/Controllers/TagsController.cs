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
        private readonly ITetriSettings _settings;
        public ITagsService TagsService;
        private ILogger<TagsController> _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="log"></param>
        public TagsController(ITetriSettings settings, ITagsService tagsService, ILogger<TagsController> log)
        {
            _settings = settings;
            TagsService = tagsService;
            _log = log;
        }

        [HttpGet("")]
        public ActionResult<string[]> GetTags()
        {
            try
            {
                return this.TagsService.ReadTagsFromIndex().ToArray();
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

        [HttpGet("{tag}/packages")]
        public ActionResult<string[]> GetTagPackages(string tag)
        {
            try
            {
                return this.TagsService.GetPackageIdsWithTag(tag).ToArray();
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

        [HttpPost("{tag}/{packageId}")]
        public ActionResult AddTag(string tag, string packageId)
        {
            try
            {
                tag = HttpUtility.UrlDecode(tag);
                this.TagsService.AddTag(packageId, tag);
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

        [HttpDelete("{tag}/{packageId}")]
        public ActionResult RemoveTag(string tag, string packageId)
        {
            try
            {
                tag = HttpUtility.UrlDecode(tag);
                this.TagsService.RemoveTag(packageId, tag);
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
    }
}
