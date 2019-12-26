using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using System.Text;
using System.Collections.Generic;

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
        public IActionResult Index()
        {
            return View(new ContentSummaryModel(_packageList.GetProjects()));
        }

        [ServiceFilter(typeof(ReadLevel))]
        [Route("projects/{project}")]
        public IActionResult Project(string project)
        {
            return View(new ProjectModel(
                _packageList.GetPopularTags(project, _settings.IndexTagListLength),
                _packageList.Get(project, 0, _settings.ListPageSize),
                project));
        }

        [ServiceFilter(typeof(ReadLevel))]
        [Route("addPackage/{project}")]
        public IActionResult AddPackage(string project)
        {
            return View(new AddPackageModel { Project = project });
        }

        [ServiceFilter(typeof(ReadLevel))]
        [Route("addProject")]
        public IActionResult AddProject()
        {
            return View();

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
            Manifest manifest = _indexReader.GetManifest(project, packageId);
            if (manifest == null)
                return View("Error404");

            return View(new PackageModel(project, packageId, manifest));
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("packages/{project}/{page?}")]
        public IActionResult Packages(string project, int page)
        {
            // user-facing page values start at 1 instead of 0, reset to 0 
            if (page != 0)
                page--;

            PageableData<Package> packages  = _packageList.GetPage(project, page, _settings.ListPageSize);

            Pager pager = new Pager();
            string pagerString = pager.Render<Package>(packages, _settings.PagesPerPageGroup, "/packages", "page");

            return View(new PackageListModel(project, pagerString, packages));
        }


        [ServiceFilter(typeof(ReadLevel))]
        [Route("packagesWithTag/{project}/{tag}")]
        public IActionResult PackagesWithTag(string project, string tag)
        {
            try
            {
                IEnumerable<Package> packages = _packageList.GetWithTag(project, tag, 0, _settings.ListPageSize);
                return View(new PackagesWithTagModel(project, tag, packages));
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

