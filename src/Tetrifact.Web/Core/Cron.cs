
using System.Threading.Tasks;

namespace Tetrifact.Web
{
    /// <summary>
    /// Base class for daemon process, these are called cron, and are the smallest unit of repeatable functionality.
    /// </summary>
    public abstract class Cron : ICron
    {
        public abstract void Start();

        public abstract Task Work();
    }
}
