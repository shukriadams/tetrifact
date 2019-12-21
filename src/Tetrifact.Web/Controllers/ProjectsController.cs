using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;
using System.Web;

namespace Tetrifact.Web
{

    /// <summary>
    /// 
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class ProjectsController : Controller
    {
        #region FIELDS

        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _log;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectService"></param>
        /// <param name="settings"></param>
        /// <param name="log"></param>
        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> log)
        {
            _projectService = projectService;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Handles posting a new package to system. 
        /// 
        /// Url : /packages/[ID]
        /// Method : POST
        /// Header
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpPost("{project}")]
        public ActionResult AddProject([FromForm]ProjectCreateArguments post)
        {
            try
            {
                post.Project = HttpUtility.UrlDecode(post.Project);
                ProjectCreateResult result = _projectService.Create(post.Project);

                // todo : add more granular error handling here
                if (result.Success)
                {
                    return Ok($"Success - project \"{post.Project}\" created.");
                } 

                return Responses.UnexpectedError(result.PublicError);
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