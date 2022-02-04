using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using System.Linq;

namespace Tetrifact.Web
{
    public class HomeController : Controller
    {
        #region FIELDS

        private readonly ISettings _settings;
        private readonly IIndexReader _indexService;
        private readonly IPackageList _packageList;

        #endregion

        #region CTORS

        public HomeController(ISettings settings, IIndexReader indexService, IPackageList packageList)
        {
            _settings = settings;
            _indexService = indexService;
            _packageList = packageList;
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
            return View();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("uploadPackage")]
        public IActionResult UploadPackage(string project)
        {
            string hostname = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            return View(new UploadPackageModel { HostName = hostname });
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ReadLevel))]
        [Route("package/{packageId}/{page?}")]
        public IActionResult Package(string packageId, int page)
        {
            ViewData["packageId"] = packageId;
            Manifest manifest = _indexService.GetManifest(packageId);
            if (manifest == null)
                return View("Error404");

            ViewData["manifest"] = manifest;

            if (page != 0)
                page--;

            Pager pager = new Pager();
            PageableData<ManifestItem> filesPage = new PageableData<ManifestItem>(manifest.Files.Skip(page * _settings.ListPageSize).Take(_settings.ListPageSize), page, _settings.ListPageSize, manifest.Files.Count);
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
            ViewData["pager"] = pager.Render(packages, _settings.PagesPerPageGroup, "/packages", "page");
            ViewData["packages"] = packages;
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
                string[] tagsSplit = tags.Split(",", System.StringSplitOptions.RemoveEmptyEntries);
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
            return View();
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [Route("error/500")]
        public IActionResult Error500()
        {
            return View();
        }


        /// <summary>
        /// Renders empty JSON response
        /// </summary>
        /// <returns></returns>
        [Route("isAlive")]
        public ActionResult IsAlive()
        {
            return new JsonResult(new
            {
                success = new
                {
                    
                }
            });
        }


        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [Route("spacecheck")]
        public ActionResult SpaceCheck()
        {
            DiskUseStats useStats = FileHelper.GetDiskUseSats();
            double freeMegabytes = FileHelper.BytesToMegabytes(useStats.FreeBytes);

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

