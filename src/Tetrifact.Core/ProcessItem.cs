using System;

namespace Tetrifact.Core
{
    public class ProcessItem
    {
        /// <summary>
        /// Unique id of process item
        /// </summary>
        public string Id { get;set; }

        /// <summary>
        /// Optional string describing process, not for logic.
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Grouping name for item. 
        /// </summary>
        public ProcessCategories Category { get; set; }

        /// <summary>
        /// If set, the time item was created. 
        /// </summary>
        public DateTime? AddedUTC { get; set; }

        /// <summary>
        /// If set, the time item was kept alive. If not set, lock never times out.
        /// </summary>
        public DateTime? KeepAliveUtc { get; set; }

        /// <summary>
        /// If set, the max age lock item can have before being auto purged.
        /// </summary>
        public TimeSpan? MaxLifespan { get; set; }

        /// <summary>
        /// Copies an instance with no object references to source.
        /// </summary>
        /// <returns></returns>
        public ProcessItem Clone() 
        { 
            return this.MemberwiseClone() as ProcessItem;
        }
    
        /// <summary>
        /// Returns a string presentation of instance, for logging purposes.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{{{this.GetType().Name} Id:{Id}, MetaData:{Metadata}, Category:{Category}, AddedUtc:{AddedUTC}, KeepAliveUtc:{KeepAliveUtc}, MaxLifespace:{MaxLifespan} }}";
        }
    }
}
