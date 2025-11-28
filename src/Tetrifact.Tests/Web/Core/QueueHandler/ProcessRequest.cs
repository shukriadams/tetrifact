using Ws = Tetrifact.Web;
using Xunit;
using Tetrifact.Core;
using Tetrifact.Web;
using System;
using System.Threading;

namespace Tetrifact.Tests.Web.Core.QueueHandler
{
    public class ProcessRequest
    {
        private TestContext _testContext = new TestContext();

        [Fact]
        public void Pass_If_No_Queue_Enforced()
        {
            Ws.QueueHandler queueHandler = _testContext.Get<Ws.QueueHandler>();
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "my-waiver");
            Assert.Equal(QueueStatus.Pass, response.Status);
            Assert.Equal("queue-not-enforced", response.Reason);
        }

        [Fact]
        public void Pass_If_IP_Whitelisted_As_Local() 
        {
            ISettings settings = new Settings();
            settings.WhiteListedLocalAddresses = new string[] { "my-local-ip" };
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            Ws.QueueHandler queueHandler = _testContext.Get<Ws.QueueHandler>("settings", settings);
            QueueResponse response = queueHandler.ProcessRequest("my-local-ip", "my-waiver");
            Assert.Equal(QueueStatus.Pass, response.Status);
            Assert.Equal("localIP", response.Reason);
        }

        [Fact]
        public void Pass_With_Waiver()
        {
            ISettings settings = new Settings();
            settings.DownloadQueueWaivers = new string[] { "my-ip" };
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            Ws.QueueHandler queueHandler = _testContext.Get<Ws.QueueHandler>("settings", settings);
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "my-waiver");
            Assert.Equal(QueueStatus.Pass, response.Status);
            Assert.Equal("waiver", response.Reason);
        }

        [Fact]
        public void Pass_AutoGenerate_Ticket()
        {
            ISettings settings = new Settings();
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            Ws.QueueHandler queueHandler = _testContext.Get<Ws.QueueHandler>("settings", settings);
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "my-waiver");
            Assert.Equal(QueueStatus.Pass, response.Status);

            // get tickets from queue, ensure that ip has been added to it
            IProcessManager processManager = _testContext.Get<IProcessManagerFactory>().GetInstance(ProcessManagerContext.ArchiveTickets);
            Assert.True(processManager.HasKey("my-ip"));
        }


        [Fact]
        public void Wait_Queue_Full()
        {
            ISettings settings = new Settings();
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            // get process manager used by queuen manager
            IProcessManager processManager = _testContext.Get<IProcessManagerFactory>().GetInstance(ProcessManagerContext.ArchiveTickets);

            // add some tickets
            TimeSpan ticketDuration = new TimeSpan(100000);
            processManager.AddUnique("other-user-ip", ticketDuration);
            Thread.Sleep(10);// wait a smidge to ensure time seperation of tickets
            processManager.AddUnique("my-ip", ticketDuration);

            Ws.QueueHandler queueHandler = _testContext.Get<Ws.QueueHandler>("settings", settings, "processManager", processManager);
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "my-waiver");
            Assert.Equal(QueueStatus.Wait, response.Status);
            Assert.Equal("inQueue", response.Reason);
        }

        [Fact]
        public void Wait_In_Queue_If_Ticket_In_Use()
        {
            ISettings settings = new Settings();
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            // get process manager used by queuen manager
            IProcessManager processManager = _testContext.Get<IProcessManagerFactory>().GetInstance(ProcessManagerContext.ArchiveTickets);
            IProcessManager activeDownloads = _testContext.Get<IProcessManagerFactory>().GetInstance(ProcessManagerContext.ArchiveActiveDownloads);

            // add a ticket, but also flag that ticket as being an active download
            processManager.AddUnique("my-ip", new TimeSpan(100000));
            activeDownloads.AddUnique("my-ip", new TimeSpan(100000));

            Ws.QueueHandler queueHandler = _testContext.Get<Ws.QueueHandler>("settings", settings, "processManager", processManager);
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "my-waiver");
            Assert.Equal(QueueStatus.Wait, response.Status);
            Assert.Equal("inQueue", response.Reason);
        }

        [Fact]
        public void Pass_Queue_Empty()
        {
            ISettings settings = new Settings();
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            // get lock manager the queue handler uses
            IProcessManager processManager = _testContext.Get<IProcessManagerFactory>().GetInstance(ProcessManagerContext.ArchiveTickets);
            // make us a ticket
            processManager.AddUnique("my-ip", new TimeSpan(100000));

            Ws.QueueHandler queueHandler = _testContext.Get<Ws.QueueHandler>("settings", settings, "processManager", processManager);
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "my-waiver");
            Assert.Equal(QueueStatus.Pass, response.Status);
            Assert.Equal("openQueue", response.Reason);
        }
    }
}
