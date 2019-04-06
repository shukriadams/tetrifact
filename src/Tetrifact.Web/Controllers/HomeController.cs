using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class HomeController : Controller
    {
        private readonly ITetriSettings _settings;
        public readonly IIndexReader IndexService;
        private readonly ILogger<HomeController> _log;
        private ITagsService _tagService;
        private PackageList _packageList;

        public HomeController(ITetriSettings settings, IIndexReader indexService, ILogger<HomeController> log, ITagsService tagService, PackageList packageList)
        {
            _settings = settings;
            IndexService = indexService;
            _log = log;
            _tagService = tagService;
            _packageList = packageList;
        }

        public IActionResult Index()
        {
            ViewData["packages"] = _packageList.Get(0, _settings.IndexPackageListLength);
            ViewData["tags"] = _packageList.GetPopularTags(10);
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
            ViewData["manifest"] = IndexService.GetManifest(packageId) ?? new Manifest();
            return View();
        }

        [Route("packagesWithTag/{tag}")]
        public IActionResult PackagesWithTag(string tag)
        {
            try
            {
                ViewData["tag"] = tag;
                ViewData["packages"] = _packageList.GetWithTag(tag, 0, _settings.IndexPackageListLength);
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
