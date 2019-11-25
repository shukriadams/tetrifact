using System.Collections.Generic;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ProjectSummaryModel
    {
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<Package> Packages { get; set; }
        public string CurrentProject { get; set; }

        public ProjectSummaryModel(IEnumerable<string> tags, IEnumerable<Package> packages, string currentProject)
        {
            this.Tags = tags;
            this.Packages = packages;
            this.CurrentProject = currentProject;
        }
    }
}
