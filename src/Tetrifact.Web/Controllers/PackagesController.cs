using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;
using System.Web;
using System.Linq;

namespace Tetrifact.Web
{

    /// <summary>
    /// - Creates and serves archives of packages.
    /// - Archive files are created on demand - this can take a while for large packages, so there
    /// can be a long wait time before the standard get method returns.
    /// - To save disk space, older archive files are cleaned out if space is limited.
    /// - For systems that cannot wait for the standard get to return, use the /order endpoint. This
    /// immediately returns a code for archive status.
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class PackagesController : Controller
    {
        #region FIELDS

        private readonly IIndexReader _indexService;
        private readonly ILogger<PackagesController> _log;
        private readonly IPackageCreate _packageCreate;
        private readonly IPackageList _packageList;
        private readonly IPackageDeleter _packageDeleter;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageCreate"></param>
        /// <param name="settings"></param>
        /// <param name="indexService"></param>
        /// <param name="settings"></param>
        /// <param name="log"></param>
        public PackagesController(IPackageCreate packageCreate, IPackageDeleter packageDeleter, IPackageList packageList, IIndexReader indexService, ILogger<PackagesController> log)
        {
            _packageDeleter = packageDeleter;
            _packageList = packageList;
            _packageCreate = packageCreate;
            _indexService = indexService;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Gets a page of packages
        /// Gets an array of all package ids 
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{project}")]
        [Produces("application/json")]
        public ActionResult ListPackages(string project, [FromQuery(Name = "isFull")] bool isFull, [FromQuery(Name = "index")] int pageIndex, [FromQuery(Name = "size")] int pageSize = 25)
        {
            try 
            {
                if (isFull)
                {
                    return Json(_packageList.Get(project, pageIndex, pageSize));
                }
                else
                {
                    return Json(_packageList.GetPackageIds(project, pageIndex, pageSize));
                }
            }
            catch (ProjectNotFoundException)
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
        /// Gets latest package with the given tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("latest/{project}/{tag}")]
        public ActionResult<Package> GetLatestPackageWithTag(string project, string tag)
        {
            try
            {
                Package package = _packageList.GetLatestWithTag(project, tag);
                if (package == null)
                    return NotFound($"Couldn't find any packages tagged with \"{tag}\". Try another tag maybe?");

                return package;
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
        /// Get rid of this, use GetPackage instead
        /// Returns 1 if the package exists, 0 if not
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{project}/{package}/exists")]
        public ActionResult<bool> PackageExists(string project, string package)
        {
            return _indexService.GetPackage(project, package) != null;
        }


        /// <summary>
        /// Returns the manifest for a package
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{project}/{package}")]
        public ActionResult GetPackage(string project, string package)
        {
            try
            {
                Package manifest = _indexService.GetPackage(project, package);
                if (manifest == null)
                    return NotFound();

                return new JsonResult(manifest);
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
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string RemoveFirstDirectoryFromPath(string path)
        {
            path = path.Replace("\\", "/");
            string[] items = path.Split("/");
            if (items.Length == 1)
                return path;

            return string.Join("/", items.Skip(1));
        }


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
        [HttpPost("{project}/{id}")]
        public ActionResult AddPackage([FromForm]PackageCreateFromPost post)
        {
            try
            {
                post.Project = HttpUtility.UrlDecode(post.Project);
                post.Id = HttpUtility.UrlDecode(post.Id);

                // check if there is space available
                DiskUseStats useStats = FileHelper.GetDiskUseSats();
                if (useStats.ToPercent() < Settings.SpaceSafetyThreshold)
                    return Responses.InsufficientSpace("Insufficient space on storage drive.");

                PackageCreateResult result = _packageCreate.Create(new PackageCreateArguments
                {
                    Description = post.Description,
                    Files = post.Files.Select(r => new PackageCreateItem { Content = r.OpenReadStream(), FileName = post.RemoveFirstDirectoryFromPath ? RemoveFirstDirectoryFromPath(r.FileName) : r.FileName}).ToList(),
                    Id = post.Id,
                    Project = post.Project,
                    IsArchive = post.IsArchive
                });

                _packageList.Clear(post.Project);
                return Ok($"Success - package \"{post.Id}\" created.");
            }
            catch (PackageCreateException ex)
            {
                if (ex.ErrorType == PackageCreateErrorTypes.InvalidArchiveFormat)
                    return Responses.InvalidArchiveFormatError();

                if (ex.ErrorType == PackageCreateErrorTypes.InvalidFileCount)
                    return Responses.InvalidArchiveContent();

                if (ex.ErrorType == PackageCreateErrorTypes.PackageExists)
                    return Responses.PackageExistsError(post.Id);

                if (ex.ErrorType == PackageCreateErrorTypes.MissingValue)
                    return Responses.MissingInputError(ex.PublicError);

                return Responses.UnexpectedError();
            } 
            catch(Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                Console.WriteLine("An unexpected error occurred : ");
                Console.WriteLine(ex);
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpDelete("{project}/{package}")]
        public ActionResult DeletePackage(string project, string package)
        {
            try
            {
                _packageDeleter.Delete(project, package);
                _packageList.Clear(project);
                return Ok();
            }
            catch (PackageLockedException) 
            {
                return Responses.PackageLockedError();
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