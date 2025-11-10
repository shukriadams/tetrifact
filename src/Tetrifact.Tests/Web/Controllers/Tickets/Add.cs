using Microsoft.AspNetCore.Mvc;
using System.Net;
using Tetrifact.Core;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Tickets
{
    public class Add : TestBase
    {
        [Fact]
        public void Skips_Ticket_Creation_If_Tickets_Not_Required()
        {
            // disable queue in settings
            Settings settings = new Settings();
            settings.MaximumSimultaneousDownloads = null;

            TicketsController controller = TestContext.Get<TicketsController>("settings", settings);
            
            JsonResult response = controller.Add(string.Empty) as JsonResult;
            Assert.Equal(200, response.StatusCode);

            dynamic responseJson = JsonHelper.ToDynamic(controller.Add(string.Empty));
            Assert.Equal((string)responseJson.success.description, "Ticket not required, none generated");
            Assert.False((bool)responseJson.success.required);
        }

        [Fact]
        public void Fails_Ticket_Creation_If_No_Request_IP()
        {
            // enable queue in settings
            Settings settings = new Settings();
            settings.MaximumSimultaneousDownloads = 1;

            TicketsController controller = TestContext.Get<TicketsController>("settings", settings);
            HttpHelper.EnsureContext(controller);
            
            JsonResult response = controller.Add(string.Empty) as JsonResult;

            Assert.Equal(500, response.StatusCode);
            dynamic responseJson = JsonHelper.ToDynamic(response);
            Assert.Equal((string)responseJson.error.description, "Failed to resolve IP");
        }


        [Fact]
        public void Happy_Path()
        {
            // enable queue in settings
            Settings settings = new Settings();
            settings.MaximumSimultaneousDownloads = 1;

            TicketsController controller = TestContext.Get<TicketsController>("settings", settings);
            HttpHelper.EnsureContext(controller);

            // ensure request has IP, else request will be ignored
            controller.ControllerContext.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("0.0.0.0");

            JsonResult response = controller.Add(string.Empty) as JsonResult;
            dynamic responseJson = JsonHelper.ToDynamic(response);


            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseJson.success.ticket);
        }
    }
}
