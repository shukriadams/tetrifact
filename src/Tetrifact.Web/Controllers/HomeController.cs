using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class HomeController : Controller
    {
        private readonly ITetriSettings _settings;
        public readonly IIndexReader IndexService;
        private readonly ILogger<HomeController> _log;
        private ITagsService _tagService;
        private IPackageList _packageList;

        public HomeController(ITetriSettings settings, IIndexReader indexService, ILogger<HomeController> log, ITagsService tagService, IPackageList packageList)
        {
            _settings = settings;
            IndexService = indexService;
            _log = log;
            _tagService = tagService;
            _packageList = packageList;
        }

        public IActionResult Index()
        {
            ViewData["packages"] = _packageList.Get(0, _settings.ListPageSize);
            ViewData["tags"] = _packageList.GetPopularTags(_settings.IndexTagListLength);
            return View();
        }

        [Route("api")]
        public IActionResult Api()
        {
            return View();
        }

        [Route("package/{packageId}")]
        public IActionResult Package(string packageId)
        {
            ViewData["packageId"] = packageId;
            Manifest manifest = IndexService.GetManifest(packageId);
            if (manifest == null)
                return View("Error404");

            ViewData["manifest"] = IndexService.GetManifest(packageId);
            return View();
        }

        [Route("packages/{page?}")]
        public IActionResult Packages(int page)
        {
            // user-facing page values start at 1 instead of 0. reset
            if (page != 0)
                page--;

            Pager pager = new Pager();
            PageableData<Package> packages  = _packageList.GetPage(page, _settings.ListPageSize);
            ViewData["pager"] = pager.Render<Package>(packages, _settings.PagesPerPageGroup, "/packages", "page");
            ViewData["packages"] = packages;
            return View();
        }

        [Route("packagesWithTag/{tag}")]
        public IActionResult PackagesWithTag(string tag)
        {
            try
            {
                ViewData["tag"] = tag;
                ViewData["packages"] = _packageList.GetWithTag(tag, 0, _settings.ListPageSize);
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

        [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}

