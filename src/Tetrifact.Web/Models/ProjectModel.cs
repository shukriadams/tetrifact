using System.Collections.Generic;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ProjectModel
    {
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<Package> Packages { get; set; }
        public string Project { get; set; }

        public ProjectModel(IEnumerable<string> tags, IEnumerable<Package> packages, string Project)
        {
            this.Tags = tags;
            this.Packages = packages;
            this.Project = Project;
        }
    }
}
