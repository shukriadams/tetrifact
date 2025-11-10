using Microsoft.AspNetCore.Mvc;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Tickets
{
    public class Delete : TestBase
    {
        [Fact]
        public void Happy_Path_Delets_Existing_Ticket()
        {
            // disable queue in settings
            IProcessManager tickets = TestContext.Get<IProcessManager>();
            tickets.AddUnique("myticket");
            IProcessManagerFactory processes = TestContext.Get<IProcessManagerFactory>();
            processes.SetInstance(ProcessManagerContext.ArchiveTickets, tickets);

            TicketsController controller = TestContext.Get<TicketsController>("processManagerFactory", processes);

            JsonResult response = controller.Delete("myticket") as JsonResult;

            Assert.Equal(200, response.StatusCode);
            Assert.False(tickets.AnyOfKeyExists("myticket"));

            dynamic responseJson = JsonHelper.ToDynamic(response);
            Assert.NotNull(responseJson.success.description);
        }

        [Fact]
        public void Returns_404_On_Invalid_Ticket()
        {
            // disable queue in settings
            IProcessManager tickets = TestContext.Get<IProcessManager>();
            tickets.AddUnique("myticket");
            IProcessManagerFactory processes = TestContext.Get<IProcessManagerFactory>();
            processes.SetInstance(ProcessManagerContext.ArchiveTickets, tickets);

            TicketsController controller = TestContext.Get<TicketsController>();

            JsonResult response = controller.Delete("a-ticket-that-hasn't-been-created") as JsonResult;

            Assert.Equal(404, response.StatusCode);
            dynamic responseJson = JsonHelper.ToDynamic(response);
            Assert.NotNull(responseJson.error.description);
        }
    }
}
