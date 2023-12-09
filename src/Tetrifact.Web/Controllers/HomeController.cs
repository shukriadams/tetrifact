using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace Tetrifact.Web
{
    public class HomeController : Controller
    {
        #region FIELDS

        private readonly ISettings _settings;
        private readonly IIndexReadService _indexService;
        private readonly IPackageListService _packageList;
        private readonly ILogger<HomeController> _log;

        #endregion

        #region CTORS

        public HomeController(ISettings settings, IIndexReadService indexService, IPackageListService packageList, ILogger<HomeController> log)
        {
            _settings = settings;
            _indexService = indexService;
            _packageList = packageList;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        public IActionResult Index()
        {
            ViewData["packages"] = _packageList.Get(0, _settings.ListPageSize);
            ViewData["tags"] = _packageList.GetPopularTags(_settings.IndexTagListLength);
            ViewData["serverName"] = _settings.ServerName;
            return View();
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("api")]
        public IActionResult Api()
        {
            IEnumerable<Package> packages = _packageList.Get(0, 10);
            string exampleTag = _packageList.GetPopularTags(1).FirstOrDefault();
            ViewData["upstreamPackageId"] = packages.Count() > 0 ? packages.ElementAt(0).Id : "my-upstream-packageId";
            ViewData["downstreamPackageId"] = packages.Count() > 1 ? packages.ElementAt(1).Id : "my-downstream-packageId";
            ViewData["exampleTag"] = string.IsNullOrEmpty(exampleTag) ? "my-example-tag" : exampleTag;
            ViewData["serverName"] = _settings.ServerName;
            return View();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("uploadPackage")]
        public IActionResult UploadPackage()
        {
            ViewData["serverName"] = _settings.ServerName;
            string hostname = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            return View(new UploadPackageModel { HostName = hostname });
        }


        /// <summary>
        /// Shows page of package files at given pageindex
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("package/{packageId}/{pageIndex?}")]
        public IActionResult Package(string packageId, int pageIndex)
        {
            ViewData["serverName"] = _settings.ServerName;
            ViewData["packageId"] = packageId;
            Manifest manifest = _indexService.GetManifest(packageId);
            if (manifest == null)
                return View("Error404");

            ViewData["manifest"] = manifest;

            if (pageIndex != 0)
                pageIndex--;

            Pager pager = new Pager();
            PageableData<ManifestItem> filesPage = new PageableData<ManifestItem>(manifest.Files.Skip(pageIndex * _settings.ListPageSize).Take(_settings.ListPageSize), pageIndex, _settings.ListPageSize, manifest.Files.Count);
            ViewData["filesPage"] = filesPage;
            ViewData["filesPager"] = pager.Render(filesPage, _settings.PagesPerPageGroup, $"/package/{packageId}", "page", "#manifestFiles");
            return View();
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("packages/{page?}")]
        public IActionResult Packages(int page)
        {
            // user-facing page values start at 1 instead of 0. reset
            if (page != 0)
                page--;

            Pager pager = new Pager();
            PageableData<Package> packages  = _packageList.GetPage(page, _settings.ListPageSize);
            ViewData["serverName"] = _settings.ServerName;
            ViewData["pager"] = pager.Render(packages, _settings.PagesPerPageGroup, "/packages", "page");
            ViewData["packages"] = packages;
            return View();
        }

        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("search/{search?}/{page?}")]
        public IActionResult Search(string search, int page)
        {
            // user-facing page values start at 1 instead of 0. reset
            if (page != 0)
                page--;

            PageableData<Package> results = _packageList.Find(search, page, _settings.ListPageSize);
            Pager pager = new Pager();
            ViewData["serverName"] = _settings.ServerName;
            ViewData["search"] = search;
            ViewData["packages"] = results;
            ViewData["pager"] = pager.Render(results, _settings.PagesPerPageGroup, "/search", "page");
            return View();
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
                string[] tagsSplit = tags.Split(",", System.StringSplitOptions.RemoveEmptyEntries).Select(r => Uri.UnescapeDataString(r)).ToArray();
                ViewData["serverName"] = _settings.ServerName;
                ViewData["tag"] = tags;
                ViewData["packages"] = _packageList.GetWithTags(tagsSplit, 0, _settings.ListPageSize);
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
            ViewData["serverName"] = _settings.ServerName;
            return View();
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [Route("error/500")]
        public IActionResult Error500()
        {
            ViewData["serverName"] = _settings.ServerName;
            return View();
        }

        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [Route("spacecheck")]
        public ActionResult SpaceCheck()
        {
            DiskUseStats useStats = _indexService.GetDiskUseSats();
            double freeMegabytes = FileHelper.BytesToMegabytes(useStats.FreeBytes);
            ViewData["serverName"] = _settings.ServerName;

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

        #endregion
    }
}

