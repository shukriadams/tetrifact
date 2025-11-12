namespace Tetrifact.Web
{
    public interface IQueueHandler
    {
        /// <summary>
        /// Processes a queue request. Queuíng is based on IP, queuing must also happen on-the-fly on archive request, so we cannot return 
        /// both the archive stream and ticket info. Therefore ticketing uses a fixed identifier (IP) that is known to both the client and
        /// server.
        /// </summary>
        /// <param name="ip">IP of user requesting.</param>
        /// <returns></returns>
        QueueResponse ProcessRequest(string ip, string waiver);
    }
}
