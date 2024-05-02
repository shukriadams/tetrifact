using System;

namespace Tetrifact.Core
{
    public class ArchiveQueueInfo
    {
        public string PackageId { get;set; }

        public DateTime QueuedUtc { get; set; }
    }
}
