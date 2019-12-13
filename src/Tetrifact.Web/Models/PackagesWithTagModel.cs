using System.Collections.Generic;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class PackagesWithTagModel
    {
        public string Tag { get; private set; }

        public string Project { get; private set; }

        public IEnumerable<Package> Packages { get; private set; }

        public PackagesWithTagModel(string project, string tag, IEnumerable<Package> packages) 
        {
            this.Project = project;
            this.Tag = tag;
            this.Packages = packages;
        }
    }
}
