using System;

namespace Tetrifact.Core
{
    /// <summary>
    /// 
    /// package not found
    /// archive available
    /// archive not available and not being generated
    /// archive being generated
    /// archive generation failed
    /// 
    /// </summary>
    public class PackageArchiveCreationStatus
    {
        public string PackageId { get;set;}

        /// <summary>
        /// If archive generation queued, time queue added.
        /// </summary>
        public DateTime? QueuedUtc { get; set; }

        /// <summary>
        /// If archive generation underway, time it was started
        /// </summary>
        public DateTime? StartedUtc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PackageArchiveCreationStates State { get;set;}

        /// <summary>
        /// If being generated, percent of archive generation complete
        /// </summary>
        public int ProgressPercent { get ; set;} 
    }
}
