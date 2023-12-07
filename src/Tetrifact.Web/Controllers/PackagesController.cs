using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

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

        private readonly IIndexReadService _indexService;
        private readonly ILogger<PackagesController> _log;
        private readonly IPackageCreateService _packageCreateService;
        private readonly IPackageListService _packageList;
        private readonly ISettings _settings;
        private readonly IPackageListCache _packageListCache;
        private readonly IPackageDiffService _packageDiffService;

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="settings"></param>
        /// <param name="indexReadService"></param>
        /// <param name="settings"></param>
        /// <param name="log"></param>
        public PackagesController(IPackageCreateService packageCreateService, IPackageListService packageListService, IPackageListCache packageListCache, IIndexReadService indexReadService, IPackageDiffService packageDiffService, ISettings settings, ILogger<PackagesController> log)
        {
            _packageList = packageListService;
            _packageCreateService = packageCreateService;
            _packageListCache = packageListCache;
            _indexService = indexReadService;
            _settings = settings;
            _packageDiffService = packageDiffService;
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
        [HttpGet("")]
        public ActionResult ListPackages([FromQuery(Name = "isFull")] bool isFull, [FromQuery(Name = "index")] int pageIndex, [FromQuery(Name = "size")] int pageSize = 25)
        {
            IEnumerable<Package> packages = _packageList.Get(pageIndex, pageSize);

            if (isFull)
            {
                return new JsonResult(new
                {
                    success = new
                    {
                        packages = packages
                    }
                });
            }
            else
            {
                return new JsonResult(new
                {
                    success = new
                    {
                        packages = packages.Select( p => p.Id)
                    }
                });
            }
        }


        /// <summary>
        /// Gets latest package with the given tag.
        /// </summary>
        /// <param name="tags">Comma-separated string of tags</param>
        /// <returns>Package for the lookup. Null if no match.</returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("latest/{tags}")]
        public ActionResult GetLatestPackageWithTag(string tags)
        {
            try
            {
                string[] tagsSplit = tags.Split(",", StringSplitOptions.RemoveEmptyEntries);
                Package package = _packageList.GetLatestWithTags(tagsSplit);

                return new JsonResult(new
                {
                    success = new
                    {
                        package = package
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
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("diff/{upstreamPackageId}/{downstreamPackageId}")]
        public ActionResult GetPackagesDiff(string upstreamPackageId, string downstreamPackageId)
        {
            try
            {
                return new JsonResult(new
                {
                    success = new
                    {
                        packagesDiff = _packageDiffService.GetDifference(upstreamPackageId, downstreamPackageId)
                    }
                });
            }
            catch (PackageNotFoundException ex)
            {
                _log.LogInformation($"{ex}");
                return Responses.NotFoundError(this, $"Package {ex.PackageId} does not exist");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
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
        [HttpGet("{packageId}/exists")]
        public ActionResult PackageExists(string packageId)
        {
            try
            {
                return new JsonResult(new
                {
                    success = new
                    {
                        exists = _indexService.PackageExists(packageId)
                    }
                });
            } 
            catch(Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpGet("{packageId}/verify")]
        public ActionResult VerifyPackage(string packageId)
        {
            try
            {
                (bool, string) result = _indexService.VerifyPackage(packageId);

                return new JsonResult(new
                {
                    success = new
                    {
                        isValid = result.Item1,
                        description = result.Item2
                    }
                });
            }
            catch (PackageNotFoundException ex)
            {
                _log.LogInformation($"{ex}");
                return Responses.NotFoundError(this, $"Package {packageId} does not exist");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }

        /// <summary>
        /// Returns the manifest for a package
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [HttpGet("{packageId}")]
        public ActionResult GetPackage(string packageId)
        {
            try
            {
                Manifest manifest = _indexService.GetManifest(packageId);
                if (manifest == null)
                    return Responses.NotFoundError(this, $"Package {packageId} does not exist");

                return new JsonResult(new
                {
                    success = new
                    {
                        package = manifest
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }


        [ServiceFilter(typeof(WriteLevel))]
        [HttpPost("createdate/{packageId}/{date}")]
        public ActionResult SetPackageCreateDate(string packageId, string date)
        { 
            try 
            { 
                _indexService.UpdatePackageCreateDate(packageId, date);

                // force flush in-memory list of packages
                _packageListCache.Clear();

                return new JsonResult(new
                {
                    success = new
                    {
                        description = "Package updated"
                    }
                });

            }
            catch (PackageNotFoundException)
            {
                return Responses.NotFoundError(this, packageId);
            }
            catch (FormatException ex)
            {
                return Responses.GeneralUserError(ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Creates a new package. Returns JSON with local hash of package.
        /// 
        /// Url : /packages/[ID]
        /// Method : POST
        /// Header
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpPost("{id}")]
        [RequestSizeLimit(long.MaxValue)]
        public ActionResult AddPackage([FromForm]PackageCreateFromPost post)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                _log.LogInformation("Package upload request started");

                // check if there is space available
                DiskUseStats useStats = _indexService.GetDiskUseSats();
                if (useStats.ToPercent() <= _settings.SpaceSafetyThreshold)
                    return Responses.InsufficientSpace("Insufficient space on storage drive.");

                // attempt to parse incoming existing files
                IEnumerable<ManifestItem> existingFiles = null;
                if (post.ExistingFiles != null)
                { 
                    try 
                    {
                        existingFiles = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<ManifestItem>>(post.ExistingFiles);
                    } 
                    catch(Exception) 
                    {
                        return Responses.InvalidJSONError();
                    }
                }

                PackageCreateResult result = _packageCreateService.Create(new PackageCreateArguments
                {
                    Description = post.Description,
                    Files = post.Files.Select(r => new PackageCreateItem { 
                        Content = r.OpenReadStream(),
                        // packages uploaded via webform are forced to include leading folder in path, these can be marked for removal
                        FileName = post.RemoveFirstDirectoryFromPath ? FileHelper.RemoveFirstDirectoryFromPath(r.FileName) : r.FileName 
                        }).ToList(),
                    Id = post.Id,
                    ExistingFiles = existingFiles,
                    IsArchive = post.IsArchive
                });

                // force flush in-memory list of packages
                if (result.Success)
                    _packageListCache.Clear();

                if (result.Success)
                {
                    // force flush in-memory list of packages
                    _packageListCache.Clear();

                    sw.Stop();

                    return new JsonResult(new
                    {
                        success = new
                        {
                            id = post.Id,
                            hash = result.PackageHash,
                            description = "Package successfully created",
                            processingTime = sw.Elapsed.TotalSeconds
                        }
                    });
                }

                if (result.ErrorType == PackageCreateErrorTypes.InvalidArchiveFormat)
                    return Responses.InvalidArchiveFormatError(post.Format);

                if (result.ErrorType == PackageCreateErrorTypes.InvalidFileCount)
                    return Responses.InvalidArchiveContent();

                if (result.ErrorType == PackageCreateErrorTypes.PackageExists)
                    return Responses.PackageAlreadyExistsError(this, post.Id);

                if (result.ErrorType == PackageCreateErrorTypes.MissingValue)
                    return Responses.MissingInputError(result.PublicError);

                return Responses.UnexpectedError();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
            finally 
            {
                _log.LogInformation($"Package request processed. {post.Id} took {0} seconds", sw.Elapsed.TotalSeconds);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(WriteLevel))]
        [HttpDelete("{packageId}")]
        public ActionResult DeletePackage(string packageId)
        {
            try
            {
                _indexService.DeletePackage(packageId);
                _packageListCache.Clear();

                return new JsonResult(new
                {
                    success = new
                    {
                        description = "Package deleted"
                    }
                });
            }
            catch (PackageNotFoundException ex)
            {
                _log.LogInformation($"{ex}");
                return Responses.NotFoundError(this, $"Package ");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred.");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPost("filterexistingfiles")]
        public ActionResult FilterExistingFiles([FromForm] PackageExistingLookupFromPost post)
        {
            Manifest incomingManifest;
            try 
            {
                incomingManifest = Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(post.Manifest);
            } 
            catch
            {
                return Responses.InvalidJSONError();
            }

            PartialPackageLookupResult re = _indexService.FindExisting(new PartialPackageLookupArguments { Files = incomingManifest.Files });

            return new JsonResult(new
            {
                success = new
                {
                    Manifest = re
                }
            });
        }

        #endregion
    }
}