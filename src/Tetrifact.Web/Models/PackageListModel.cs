using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class PackageListModel
    {
        public string Project { get; private set; }
        public PageableData<Package> Packages { get; private set; }
        public string Pager { get; private set; }

        public PackageListModel(string project, string pager, PageableData<Package> packages) 
        {
            this.Project = project;
            this.Packages = packages;
            this.Pager = pager;
        }
    }
}
