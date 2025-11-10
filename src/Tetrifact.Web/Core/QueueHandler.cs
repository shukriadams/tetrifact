using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class QueueHandler : IQueueHandler
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly IProcessManager _ticketManager;

        private readonly IProcessManager _activeDownloadsTracker;

        #endregion

        #region CTORS

        public QueueHandler(ISettings settings, IProcessManagerFactory processManagerFactory) 
        {
            _settings = settings;
            _ticketManager = processManagerFactory.GetInstance(ProcessManagerContext.ArchiveTickets);
            _activeDownloadsTracker = processManagerFactory.GetInstance(ProcessManagerContext.ArchiveActiveDownloads);
        }

        #endregion

        #region METHODS

        public virtual QueueResponse ProcessRequest(string ip, string ticket, string waiver) 
        {
            if (!_settings.MaximumSimultaneousDownloads.HasValue)
                return new QueueResponse { Status = QueueStatus.Pass, Reason = "queue-not-enforced" };

            // local (this website) downloads always allowed.
            if (_settings.WhiteListedLocalAddresses.Contains(ip.ToLower()))
                return new QueueResponse { Status = QueueStatus.Pass, Reason = "localIP" };

            if (_settings.DownloadQueuePriorityTickets.Contains(waiver))
                return new QueueResponse { Status = QueueStatus.Pass, Reason = "waiver" };

            IEnumerable<ProcessItem> userTickets = _ticketManager.GetAll();
            ProcessItem userTicket = userTickets.FirstOrDefault(i => i.Id == ticket);
            if (userTicket == null)
                return new QueueResponse { Status = QueueStatus.Deny, Reason = "invalidTicket" };

            // ticket is already being used, wait for it to exit
            if (_activeDownloadsTracker.AnyOfKeyExists(ticket))
                return new QueueResponse { Reason = "inQueue", Status = QueueStatus.Wait };

            // if there are older tickets than the one user has, queue user.
            int count = userTickets.Where(t => t.AddedUTC < userTicket.AddedUTC).Count();
            if (count >= _settings.MaximumSimultaneousDownloads)
                return new QueueResponse { Reason = "inQueue", Status = QueueStatus.Wait, WaitPosition = count - _settings.MaximumSimultaneousDownloads.Value, QueueLength = count };

            return new QueueResponse { Status = QueueStatus.Pass, Reason = "openQueue" };
        }

        #endregion
    }
}
