using System;
using System.Linq;
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

        public virtual QueueResponse ProcessRequest(string address, string waiver) 
        {
            if (string.IsNullOrEmpty(waiver))
                waiver = string.Empty;

            if (string.IsNullOrEmpty(address))
                address = string.Empty;

            if (!_settings.MaximumSimultaneousDownloads.HasValue)
                return new QueueResponse { 
                    Status = QueueStatus.Pass, 
                    Reason = "queue-not-enforced" 
                };

            // local (this website) downloads always allowed.
            if (_settings.WhiteListedLocalAddresses.Contains(address.ToLower()))
                return new QueueResponse { 
                    Status = QueueStatus.Pass, 
                    IsLocal= true, 
                    Reason = "localIP" 
                };

            // does user have a golden persistent ticket. These tickets are registered here on server in config,
            // and can be used by users to waive the queue.
            if (_settings.DownloadQueueWaivers.Contains(address.ToLower()))
                return new QueueResponse { 
                    Status = QueueStatus.Pass, 
                    Reason = "waiver" 
                };

            // does user have a golden persistent ticket. These tickets are registered here on server in config,
            // and can be used by users to waive the queue.
            if (_settings.DownloadQueueWaivers.Select(w => w.ToLower()).Contains(waiver.ToLower()))
                return new QueueResponse
                {
                    Status = QueueStatus.Pass,
                    Reason = "waiver"
                };

            // ticket is already associated with active download, wait for that download to exit
            if (_activeDownloadsTracker.HasKey(address))
                return new QueueResponse { 
                    Reason = "inQueue", 
                    Status = QueueStatus.Wait 
                };

            // create ticket for this ip if one does not already exist
            ProcessItem userTicket = _ticketManager.TryFind(address);
            if (userTicket == null) 
                userTicket = _ticketManager.AddUnique(
                    address,
                    new TimeSpan(0, 0, _settings.DownloadQueueTicketLifespan),
                    true);

            // if there are older tickets than the one user has, queue user.
            int aheadInLine = _ticketManager.GetAll().Where(t => t.AddedUTC < userTicket.AddedUTC).Count();
            if (aheadInLine >= _settings.MaximumSimultaneousDownloads)
                return new QueueResponse { 
                    Reason = "inQueue",
                    Status = QueueStatus.Wait, 
                    QueueLength = aheadInLine 
                };

            return new QueueResponse { 
                Status = QueueStatus.Pass,
                Reason = "openQueue" 
            };
        }

        #endregion
    }
}
