using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PackageItem
    {
        public string Path { get; set; }

        public string Hash { get; set; }

        public string Id { get; set; }

        public long Size { get; set; }

        public IList<PackageItemChunk> Chunks { get; private set; }

        public PackageItem()
        {
            this.Chunks = new List<PackageItemChunk>();
        }

    }
}
