using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Tests
{
    public  class HttpHelper
    {
        public static void EnsureContext(Controller controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }
    }
}
