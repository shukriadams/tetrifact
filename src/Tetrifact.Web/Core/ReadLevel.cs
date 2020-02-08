using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ReadLevel : IActionFilter
    {
         public void OnActionExecuted(ActionExecutedContext context)
        {
            // do nothing
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (Settings.AuthorizationLevel == AuthorizationLevel.None || Settings.AuthorizationLevel > AuthorizationLevel.Read)
                return;

            context.HttpContext.Response.StatusCode = 403;

            context.Result = new ViewResult
            {
                ViewName = "~/Views/Home/Error403.cshtml"
            };
        }
    }
}
