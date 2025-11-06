using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class PackagePageModel
    {
        public string PackageId { get; set; }
        public Manifest Manifest { get; set; }
        public PageableData<ManifestItem> FilesPage { get; set; }
        public ArchiveProgressInfo ArchiveGenerationStatus { get; set; }
        public string Pager { get; set; }
        public string Title { get; set; }
    }
}
