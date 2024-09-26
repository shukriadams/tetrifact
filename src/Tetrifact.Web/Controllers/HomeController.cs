using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace Tetrifact.Web
{
    public class HomeController : Controller
    {
        #region FIELDS

        private readonly ISettings _settings;
        private readonly IIndexReadService _indexService;
        private readonly IPackageListService _packageList;
        private readonly ILogger<HomeController> _log;
        private readonly IProcessLockManager _processes;
        private readonly IArchiveService _archiveService;
        private readonly IMemoryCache _cache;

        #endregion

        #region CTORS

        public HomeController(ISettings settings, IProcessLockManager processes, IMemoryCache cache, IArchiveService archiveService, IIndexReadService indexService, IPackageListService packageList, ILogger<HomeController> log)
        {
            _settings = settings;
            _indexService = indexService;
            _packageList = packageList;
            _log = log;
            _processes = processes;
            _cache = cache;
            _archiveService = archiveService;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("summary")]
        public IActionResult Summary()
        {
            try 
            {
                ViewData["layoutViewModel"] = new LayoutViewModel { 
                    ServerName = _settings.ServerName, 
                    PageTitle = "Overview", 
                    ServerSecondaryName = _settings.ServerSecondaryName, 
                    ThemeClass = _settings.ThemeClass };

                ViewData["packages"] = _packageList.Get(0, _settings.ListPageSize);
                ViewData["tags"] = _packageList.GetPopularTags(_settings.IndexTagListLength);
                ViewData["settings"] = _settings;

                return View();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("api")]
        public IActionResult Api()
        {
            try
            {
                IEnumerable<Package> packages = _packageList.Get(0, 10);
                string exampleTag = _packageList.GetPopularTags(1).FirstOrDefault();
                
                ViewData["upstreamPackageId"] = packages.Count() > 0 ? packages.ElementAt(0).Id : "my-upstream-packageId";
                ViewData["downstreamPackageId"] = packages.Count() > 1 ? packages.ElementAt(1).Id : "my-downstream-packageId";
                ViewData["exampleTag"] = string.IsNullOrEmpty(exampleTag) ? "my-example-tag" : exampleTag;
                ViewData["layoutViewModel"] = new LayoutViewModel {
                    PageTitle = "API Reference",
                    ThemeClass = _settings.ThemeClass,
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName
                };

                return View();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("processes")]
        public IActionResult Processes()
        {
            try
            {
                IEnumerable<ProcessLockItem> processes = _processes.GetCurrent();
                ViewData["processes"] = processes;
                ViewData["layoutViewModel"] = new LayoutViewModel {
                    PageTitle = "Processes",
                    ThemeClass = _settings.ThemeClass,
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName
                };

                return View();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("uploadPackage")]
        public IActionResult UploadPackage()
        {
            try
            {
                string hostname = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                ViewData["layoutViewModel"] = new LayoutViewModel { 
                    PageTitle = "Upload Package",
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName,
                    ThemeClass = _settings.ThemeClass
                };

                return View(new UploadPackageModel { HostName = hostname });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Shows page of package files at given pageindex
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("package/{packageId}")]
        public IActionResult Package(string packageId, [FromQuery(Name = "pageIndex")] int pageIndex)
        {
            try
            {
                Manifest manifest = _indexService.GetManifest(packageId);
                LayoutViewModel layoutViewModel = new LayoutViewModel { 
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName,
                    ThemeClass = _settings.ThemeClass
                };

                if (manifest == null)
                {
                    layoutViewModel.PageTitle = "Not Found";
                    ViewData["layoutViewModel"] = layoutViewModel;
                    return View("Error404");
                }

                ArchiveProgressInfo archiveGenerationStatus = _archiveService.GetPackageArchiveStatus(packageId);
                if (pageIndex != 0)
                    pageIndex--;

                Pager pager = new Pager();
                PageableData<ManifestItem> filesPage = new PageableData<ManifestItem>(manifest.Files.Skip(pageIndex * _settings.ListPageSize).Take(_settings.ListPageSize), pageIndex, _settings.ListPageSize, manifest.Files.Count);

                layoutViewModel.PageTitle = $"Package : {manifest.Id}";

                ViewData["filesPage"] = filesPage;
                ViewData["filesPager"] = pager.Render(filesPage, _settings.PagesPerPageGroup, $"/package/{packageId}", "page", "#manifestFiles");
                ViewData["layoutViewModel"] = layoutViewModel;
                ViewData["packageId"] = packageId;
                ViewData["manifest"] = manifest;
                ViewData["archiveGenerationStatus"] = archiveGenerationStatus;

                return View();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("archiveStatus/{packageId}")]
        public IActionResult ArchiveStatus(string packageId)
        {
            try
            {
                string key = _archiveService.GetArchiveProgressKey(packageId);
                ArchiveProgressInfo archiveGenerationStatus = _cache.Get<ArchiveProgressInfo>(key);

                ViewData["packageId"] = packageId;
                ViewData["layoutViewModel"] = new LayoutViewModel { 
                    PageTitle = "Archive status",
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName,
                    ThemeClass = _settings.ThemeClass
                };

                return PartialView("~/Views/Shared/ArchiveProgress.cshtml", archiveGenerationStatus);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("{page?}")]
        public IActionResult Index([FromQuery(Name = "page")] int page)
        {
            try
            {
                // user-facing page values start at 1 instead of 0. reset
                if (page != 0)
                    page--;

                Pager pager = new Pager();
                PageableData<Package> packages  = _packageList.GetPage(page, _settings.ListPageSize);
                
                ViewData["pager"] = pager.Render(packages, _settings.PagesPerPageGroup, "/packages", "page");
                ViewData["packages"] = packages;
                ViewData["settings"] = _settings;
                ViewData["layoutViewModel"] = new LayoutViewModel { 
                    PageTitle = string.Empty,
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName,
                    ThemeClass = _settings.ThemeClass
                };

                return View();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("search/{search?}/{page?}")]
        public IActionResult Search(string search, [FromQuery(Name = "page")] int page)
        {
            try
            {
                // user-facing page values start at 1 instead of 0. reset
                if (page != 0)
                    page--;

                if (search == null)
                    search = string.Empty;

                PageableData<Package> results = _packageList.Find(search, page, _settings.ListPageSize);
                Pager pager = new Pager();

                ViewData["themeClass"] = _settings.ThemeClass;
                ViewData["packages"] = results;
                ViewData["pager"] = pager.Render(results, _settings.PagesPerPageGroup, $"/search/{search}", "page");
                ViewData["settings"] = _settings;
                ViewData["search"] = search;
                ViewData["layoutViewModel"] = new LayoutViewModel { 
                    PageTitle = string.Empty,
                    Search = search,
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName,
                    ThemeClass = _settings.ThemeClass
                };

                return View();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Responses.UnexpectedError();
            }
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("packagesWithTag/{tags}")]
        public IActionResult PackagesWithTag(string tags)
        {
            try
            {
                string[] tagsSplit = tags.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(r => Uri.UnescapeDataString(r)).ToArray();

                ViewData["tag"] = tags;
                ViewData["packages"] = _packageList.GetWithTags(tagsSplit, 0, _settings.ListPageSize);
                ViewData["layoutViewModel"] = new LayoutViewModel { 
                    PageTitle = $"Packages tagged with \"{tags}\"",
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName,
                    ThemeClass = _settings.ThemeClass
                };

                return View();
            }
            catch (TagNotFoundException)
            {
                return NotFound();
            }
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [Route("error/404")]
        public IActionResult Error404()
        {
            try
            {
                ViewData["layoutViewModel"] = new LayoutViewModel {
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName,
                    ThemeClass = _settings.ThemeClass
                };

                return View();
            }
            catch (TagNotFoundException)
            {
                return NotFound();
            }
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [Route("error/500")]
        public IActionResult Error500()
        {
            try
            {
                ViewData["layoutViewModel"] = new LayoutViewModel {
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName,
                    ThemeClass = _settings.ThemeClass
                };

                return View();
            }
            catch (TagNotFoundException)
            {
                return NotFound();
            }
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [Route("spacecheck")]
        public ActionResult SpaceCheck()
        {
            try
            {
                DiskUseStats useStats = _indexService.GetDiskUseSats();
                double freeMegabytes = FileHelper.BytesToMegabytes(useStats.FreeBytes);
                ViewData["layoutViewModel"] = new LayoutViewModel {
                    ServerName = _settings.ServerName,
                    ServerSecondaryName = _settings.ServerSecondaryName,
                    ThemeClass = _settings.ThemeClass
                };

                return new JsonResult(new
                {
                    success = new
                    {
                        total = $"{FileHelper.BytesToMegabytes(useStats.TotalBytes)}M",
                        available = $"{freeMegabytes}M ({useStats.ToPercent()}%)",
                        safetyExceeded = freeMegabytes < _settings.SpaceSafetyThreshold
                    }
                });
            }
            catch (TagNotFoundException)
            {
                return NotFound();
            }
        }

        #endregion
    }
}