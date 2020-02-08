using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class PackageModel
    {
        public string Project { get; private set; }
        public string PackageId { get; private set; }
        public Package Manifest { get; private set; }

        public PackageModel(string project, string packageId, Package manifest)
        {
            this.Project = project;
            this.PackageId = packageId;
            this.Manifest = manifest;
        }
    }
}
