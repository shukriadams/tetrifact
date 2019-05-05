using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class WriteLevel : IActionFilter
    {
        private readonly ITetriSettings _settings;

        public WriteLevel(ITetriSettings settings)
        {
            _settings = settings;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_settings.AuthorizationLevel == AuthorizationLevel.None || _settings.AuthorizationLevel > AuthorizationLevel.Write)
                return;

            string authmethod = context.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authmethod))
            {
                authmethod = authmethod.Trim();
                if (authmethod.StartsWith("token "))
                {
                    string token = authmethod.Substring(5).Trim();
                    if (_settings.AccessTokens.Contains(token))
                        return;
                }
            }

            context.HttpContext.Response.StatusCode = 403;

            context.Result = new ViewResult
            {
                ViewName = "~/Views/Home/Error403.cshtml"
            };
        }
    }
}
