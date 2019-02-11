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

        public HomeController(ITetriSettings settings, IIndexReader indexService, ILogger<HomeController> log, ITagsService tagService)
        {
            _settings = settings;
            IndexService = indexService;
            _log = log;
            _tagService = tagService;

        }

        public IActionResult Index()
        {
            ViewData["packagedIds"] = this.IndexService.GetPackages();
            ViewData["tags"] = this._tagService.GetTags();
            return View();
        }

        [Route("api")]
        public IActionResult Api()
        {
            return View();
        }

        [Route("packages/{packageId}")]
        public IActionResult Packages(string packageId)
        {
            ViewData["packageId"] = packageId;
            ViewData["manifest"] = IndexService.GetManifest(packageId) ?? new Manifest();
            return View();
        }

        [Route("packagesWithTag/{tag}")]
        public IActionResult PackagesWithTag(string tag)
        {
            ViewData["tag"] = tag;
            ViewData["packageIds"] = _tagService.GetPackagesWithTag(tag);
            return View();
        }
    }
}
