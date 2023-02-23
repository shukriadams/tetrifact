using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    [Route("v1/[controller]")]
    [ApiController]
    public class PruneController
    {
        private readonly IPackagePruneService _pruneService;

        public PruneController(IPackagePruneService pruneService)
        {
            _pruneService = pruneService;
        }

        [Route("report")]
        public string Report()
        {
            PruneReport report = _pruneService.Report();
            string s = string.Empty;
            foreach(string l in report.Report)
                s += l+ "\n";

            return s;
        }
    }
}
