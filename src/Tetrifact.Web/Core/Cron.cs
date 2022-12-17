
namespace Tetrifact.Web
{
    public abstract class Cron : ICron
    {
        public string CronMask { get; set; }

        public Cron()
        {
            this.CronMask = "* * * * *";
        }
    
        public abstract void Start();

        public abstract void Work();
    }
}
