using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tetrifact.Web
{
    /// <summary>
    /// Checks global state to determine if server has errors. If errors, forces all view calls to display configuration error page
    /// </summary>
    public class ConfigurationErrors: IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do nothing
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (AppState.ConfigErrors)
            {
                context.HttpContext.Response.StatusCode = 500;
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Errors/Configuration.cshtml",

                };
            }
        }
    }
}