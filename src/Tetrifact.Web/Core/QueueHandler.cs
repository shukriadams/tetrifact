using System.Collections.Generic;
using System.Linq;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class QueueHandler : IQueueHandler
    {
        #region FIELDS

        private ISettings _settings;

        private IProcessManager _processManager;

        #endregion

        #region CTORS

        public QueueHandler(ISettings settings, IProcessManager processManager) 
        {
            _settings = settings;
            _processManager = processManager;
        }

        #endregion
        
        #region METHODS

        public virtual QueueResponse ProcessRequest(string ip, string ticket, string waiver) 
        {
            // local (this website) downloads always allowed.
            if (_settings.WhiteListedLocalAddresses.Contains(ip.ToLower()))
                return new QueueResponse { Status = QueueStatus.Pass, Reason = "local" };

            if (!_settings.MaximumSimultaneousDownloads.HasValue)
                return new QueueResponse { Status = QueueStatus.Pass, Reason = "queue-not-enforced" };

            if (_settings.DownloadQueuePriorityTickets.Contains(waiver))
                return new QueueResponse { Status = QueueStatus.Pass, Reason = "waiver" };

            IEnumerable<ProcessItem> userTickets = _processManager.GetByCategory(ProcessCategories.ArchiveQueueSlot);
            ProcessItem userTicket = userTickets.FirstOrDefault(i => i.Id == ticket);
            if (userTicket == null)
                return new QueueResponse { Status = QueueStatus.Deny, Reason = "invalid ticket" };

            // if there are older tickets than the one user has, queue user.
            int count = userTickets.Where(t => t.AddedUTC < userTicket.AddedUTC).Count();
            if (count > _settings.MaximumSimultaneousDownloads)
                return new QueueResponse { Reason = "waiting in queue", Status = QueueStatus.Wait, WaitPosition = count - _settings.MaximumSimultaneousDownloads.Value, QueueLength = count, };

            return new QueueResponse { Status = QueueStatus.Pass, Reason = "queue open" };
        }

        #endregion
    }
}
