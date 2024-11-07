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
        [ServiceFilter(typeof(ConfigurationErrors))]
        [ServiceFilter(typeof(ReadLevel))]
        public string Report()
        {
            PrunePlan report = _pruneService.GeneratePrunePlan();
            string s = string.Empty;
            foreach(string l in report.Report)
                s += l+ "\n";

            return s;
        }
    }
}
