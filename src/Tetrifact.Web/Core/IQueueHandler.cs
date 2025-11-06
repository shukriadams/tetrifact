namespace Tetrifact.Web
{
    public interface IQueueHandler
    {
        QueueResponse ProcessRequest(string ip, string ticket, string waiver);
    }
}
