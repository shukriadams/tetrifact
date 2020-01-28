using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class ManifestItem
    {
        public string Path { get; set; }

        public string Hash { get; set; }

        public string Id { get; set; }

        public long Size { get; set; }

        public IList<ManifestItemChunk> Chunks { get; private set; }

        public ManifestItem()
        {
            this.Chunks = new List<ManifestItemChunk>();
        }

    }
}
