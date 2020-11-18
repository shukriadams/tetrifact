using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tetrifact.Core;
using System.Linq;


namespace Tetrifact.Web
{
    public class ReadLevel : IActionFilter
    {
        private readonly ITetriSettings _settings;

        public ReadLevel(ITetriSettings settings)
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

            // allow of no authtokens defined in settings
            if (!_settings.AccessTokens.Any())
                return;

            // allow if http Authorization header contains token defined in settings
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
