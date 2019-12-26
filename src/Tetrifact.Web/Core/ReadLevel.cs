using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ReadLevel : IActionFilter
    {
        private readonly ISettings _settings;

        public ReadLevel(ISettings settings)
        {
            _settings = settings;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do nothing
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_settings.AuthorizationLevel == AuthorizationLevel.None || _settings.AuthorizationLevel > AuthorizationLevel.Read)
                return;

            context.HttpContext.Response.StatusCode = 403;

            context.Result = new ViewResult
            {
                ViewName = "~/Views/Home/Error403.cshtml"
            };
        }
    }
}
