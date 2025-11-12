namespace Tetrifact.Web
{

    public class QueueResponse
    {
        public QueueStatus Status { get; set; }

        public int QueueLength { get; set; }

        public bool IsLocal { get; set; }
       
        /// <summary>
        /// for logging. Normally "local ip"|"waiver"|"available capacity"
        /// </summary>
        public string Reason { get; set; }
    }
}
