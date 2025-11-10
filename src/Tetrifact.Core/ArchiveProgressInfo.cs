using System;
using System.Text.Json.Serialization;

namespace Tetrifact.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class ArchiveProgressInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// If archive generation queued, time queue added.
        /// </summary>
        public DateTime QueuedUtc { get; set; }

        /// <summary>
        /// If archive generation underway, time it was started
        /// </summary>
        public DateTime? StartedUtc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PackageArchiveCreationStates State { get;set;}

        /// <summary>
        /// 
        /// </summary>
        public int PercentProgress { get ; set;}

        public long ProjectedSize { get; set; }

        public ArchiveProgressInfo() 
        {
            this.State = PackageArchiveCreationStates.Processed_ArchiveNotGenerated;
        }
    }
}
