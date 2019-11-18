using System.Collections.Generic;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ContentSummaryModel
    {
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<Package> Packages { get; set; }
        public IEnumerable<string> Projects { get; set; }
        public string CurrentProject { get; set; }

        public ContentSummaryModel(IEnumerable<string> tags, IEnumerable<Package> packages, IEnumerable<string> projects, string currentProject) 
        {
            this.Tags = tags;
            this.Packages = packages;
            this.Projects = projects;
            this.CurrentProject = currentProject;
        }
    }
}
