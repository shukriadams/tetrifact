using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using System.Text;

namespace Tetrifact.Web
{
    public class HomeController : Controller
    {
        #region FIELDS

        private readonly ITetriSettings _settings;
        private readonly IIndexReader _indexReader;
        private readonly IPackageList _packageList;

        #endregion

        #region CTORS

        public HomeController(ITetriSettings settings, IIndexReader indexReader, IPackageList packageList)
        {
            _settings = settings;
            _indexReader = indexReader;
            _packageList = packageList;
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(ReadLevel))]
        [Route("")]
        public IActionResult Index(string project)
        {
            return View(new ContentSummaryModel(
                _packageList.GetPopularTags(project, _settings.IndexTagListLength),
                _packageList.Get(project, 0, _settings.ListPageSize),
                _indexReader.GetProjects(),
                project));
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("api")]
        public IActionResult Api()
        {
            return View();
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("package/{project}/{packageId}")]
        public IActionResult Package(string project, string packageId)
        {
            ViewData["packageId"] = packageId;
            Manifest manifest = _indexReader.GetManifest(project, packageId);
            if (manifest == null)
                return View("Error404");

            ViewData["manifest"] = _indexReader.GetManifest(project, packageId);
            return View();
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("packages/{project}/{page?}")]
        public IActionResult Packages(string project, int page)
        {
            // user-facing page values start at 1 instead of 0. reset
            if (page != 0)
                page--;

            Pager pager = new Pager();
            PageableData<Package> packages  = _packageList.GetPage(project, page, _settings.ListPageSize);
            ViewData["pager"] = pager.Render<Package>(packages, _settings.PagesPerPageGroup, "/packages", "page");
            ViewData["packages"] = packages;
            return View();
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("packagesWithTag/{project}/{tag}")]
        public IActionResult PackagesWithTag(string project, string tag)
        {
            try
            {
                ViewData["tag"] = tag;
                ViewData["packages"] = _packageList.GetWithTag(project, tag, 0, _settings.ListPageSize);
                return View();
            }
            catch (TagNotFoundException)
            {
                return NotFound();
            }
        }


        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();
        }

        [Route("error/500")]
        public IActionResult Error500()
        {
            return View();
        }

        [Route("isAlive")]
        public IActionResult IsAlive()
        {
            return Ok("200");
        }

        [Route("spacecheck")]
        public IActionResult SpaceCheck()
        {
            DiskUseStats useStats = FileHelper.GetDiskUseSats();
            double freeMegabytes = FileHelper.BytesToMegabytes(useStats.FreeBytes);

            StringBuilder s = new StringBuilder();
            s.AppendLine($"Drive size : {FileHelper.BytesToMegabytes(useStats.TotalBytes)}M");
            s.AppendLine($"Space available :  {freeMegabytes}M ({useStats.ToPercent()}%)");

            if (freeMegabytes > _settings.SpaceSafetyThreshold){
                return Ok(s.ToString());
            }

            s.AppendLine($"Insufficient space for safe operation - minimum allowed is {_settings.SpaceSafetyThreshold}M.");

            return Responses.InsufficientSpace(s.ToString());
        }

        #endregion
    }
}

