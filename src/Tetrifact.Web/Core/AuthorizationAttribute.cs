using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Tetrifact.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizationAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly ActionLevel _level;

        public AuthorizationAttribute(ActionLevel level)
        {
            _level = level;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                // it isn't needed to set unauthorized result 
                // as the base class already requires the user to be authenticated
                // this also makes redirect to a login page work properly
                // context.Result = new UnauthorizedResult();
                return;
            }

            bool isAuthorized = false;
            if (!isAuthorized)
            {
                context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
                return;
            }
        }
    }
}
