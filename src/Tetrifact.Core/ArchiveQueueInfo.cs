using System;

namespace Tetrifact.Core
{
    public class ArchiveQueueInfo
    {
        public string PackageId { get;set; }

        public DateTime CreatedUtc { get; set; }

        public long ProjectedSize { get; set; }


    }
}
