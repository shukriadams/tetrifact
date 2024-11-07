using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Web
{
    public class ErrorsController : Controller
    {
        [Route("errors/configuration")]
        public IActionResult Configuration()
        {
            this.HttpContext.Response.StatusCode = 500;
            return View();
        }
    }
}
