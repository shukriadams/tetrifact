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

        /// <summary>
        /// Gets a list of all tags currently in use.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("")]
        public ActionResult GetTags()
        {
            try
            {
                return new JsonResult(new
                {
                    success = new
                    {
                        tags = _tagsService.GetAllTags().ToArray()
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// GEts a list of package ids which have the given tags.
        /// </summary>
        /// <param name="tags">Comma-separated list of tags.</param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{tags}/packages")]
        public ActionResult GetTagPackages(string tags)
        {
            try
            {
                string[] tagsSplit = tags.Split(",", StringSplitOptions.RemoveEmptyEntries);

                return new JsonResult(new
                {
                    success = new
                    {
                        packages = _tagsService.GetPackageIdsWithTags(tagsSplit).ToArray()
                    }
                });

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Adds a tag to the given package.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpPost("{tag}/{packageId}")]
        public ActionResult AddTag(string tag, string packageId)
        {
            try
            {
                tag = HttpUtility.UrlDecode(tag);
                _tagsService.AddTag(packageId, tag);

                return new JsonResult(new
                {
                    success = new
                    {
                        description = $"Tag {tag} was added to package {packageId}"
                    }
                });
            }
            catch (PackageNotFoundException ex)
            {
                _log.LogInformation($"{ex}");
                return Responses.NotFoundError(this, $"Package {packageId} not found.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Removes tag from the given tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpDelete("{tag}/{packageId}")]
        public ActionResult RemoveTag(string tag, string packageId)
        {
            try
            {
                tag = HttpUtility.UrlDecode(tag);
                _tagsService.RemoveTag(packageId, tag);

                return new JsonResult(new
                {
                    success = new
                    {
                        description = $"Tag {tag} was removed from package {packageId}"
                    }
                });

            }
            catch (PackageNotFoundException ex)
            {
                _log.LogInformation($"{ex}");
                return Responses.NotFoundError(this, $"Package {packageId} not found.");
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
