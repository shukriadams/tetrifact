using Microsoft.AspNetCore.Mvc.Filters;
using Tetrifact.Core;

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
            string test = "";
            //context.HttpContext.Response.Redirect("/denied");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

            string test = "";

        }
    }
}
