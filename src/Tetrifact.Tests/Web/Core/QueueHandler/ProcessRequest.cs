using Ws = Tetrifact.Web;
using Xunit;
using Tetrifact.Core;
using Tetrifact.Web;
using System;
using System.Threading;

namespace Tetrifact.Tests.Web.Core.QueueHandler
{
    public class ProcessRequest: TestBase
    {

        [Fact]
        public void Pass_If_No_Queue_Enforced()
        {
            Ws.QueueHandler queueHandler = this.TestContext.Get<Ws.QueueHandler>();
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "some-ticket", "some-waiver");
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

            Ws.QueueHandler queueHandler = this.TestContext.Get<Ws.QueueHandler>("settings", settings);
            QueueResponse response = queueHandler.ProcessRequest("my-local-ip", "some-ticket", "some-waiver");
            Assert.Equal(QueueStatus.Pass, response.Status);
            Assert.Equal("localIP", response.Reason);
        }

        [Fact]
        public void Pass_With_Waiver()
        {
            ISettings settings = new Settings();
            settings.DownloadQueuePriorityTickets = new string[] { "my-waiver" };
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            Ws.QueueHandler queueHandler = this.TestContext.Get<Ws.QueueHandler>("settings", settings);
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "some-ticket", "my-waiver");
            Assert.Equal(QueueStatus.Pass, response.Status);
            Assert.Equal("waiver", response.Reason);
        }

        [Fact]
        public void Fail_No_Ticket()
        {
            ISettings settings = new Settings();
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            Ws.QueueHandler queueHandler = this.TestContext.Get<Ws.QueueHandler>("settings", settings);
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "some-ticket", "some-waiver");
            Assert.Equal(QueueStatus.Deny, response.Status);
            Assert.Equal("invalidTicket", response.Reason);
        }


        [Fact]
        public void Wait_Queue_Full()
        {
            ISettings settings = new Settings();
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            IProcessManager processManager = this.TestContext.Get<IProcessManager>();
            TimeSpan ticketDuration = new TimeSpan(100000);
            processManager.AddByCategory(ProcessCategories.ArchiveQueueSlot, ticketDuration, "other-user-ticket", string.Empty);
            Thread.Sleep(10);// wait a smidge to ensure time seperation of tickets
            processManager.AddByCategory(ProcessCategories.ArchiveQueueSlot, ticketDuration, "my-ticket", string.Empty);

            Ws.QueueHandler queueHandler = this.TestContext.Get<Ws.QueueHandler>("settings", settings, "processManager", processManager);
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "my-ticket", "some-waiver");
            Assert.Equal(QueueStatus.Wait, response.Status);
            Assert.Equal("inQueue", response.Reason);
        }

        [Fact]
        public void Pass_Queue_Empty()
        {
            ISettings settings = new Settings();
            // force some queue value to enforce it
            settings.MaximumSimultaneousDownloads = 1;

            IProcessManager processManager = this.TestContext.Get<IProcessManager>();
            TimeSpan ticketDuration = new TimeSpan(100000);
            processManager.AddByCategory(ProcessCategories.ArchiveQueueSlot, ticketDuration, "my-ticket", string.Empty);

            Ws.QueueHandler queueHandler = this.TestContext.Get<Ws.QueueHandler>("settings", settings, "processManager", processManager);
            QueueResponse response = queueHandler.ProcessRequest("my-ip", "my-ticket", "some-waiver");
            Assert.Equal(QueueStatus.Pass, response.Status);
            Assert.Equal("openQueue", response.Reason);
        }
    }
}
