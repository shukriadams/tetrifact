using System;

namespace Tetrifact.Core
{
    public class ProcessLockItem
    {
        /// <summary>
        /// Unique id of lock
        /// </summary>
        public string Id { get;set; }

        /// <summary>
        /// Grouping name for item. 
        /// </summary>
        public ProcessLockCategories Category { get; set; }

        /// <summary>
        /// If set, the time lock item was created. If not set, lock never times out
        /// </summary>
        public DateTime? AddedUTC { get; set; }

        /// <summary>
        /// If set, the max age lock item can have before being autopurged.
        /// </summary>
        public TimeSpan? MaxLifespan { get; set; }

        public ProcessLockItem Clone() 
        { 
            return this.MemberwiseClone() as ProcessLockItem;
        }
    }
}
