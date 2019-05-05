using Microsoft.AspNetCore.Mvc.Filters;
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

        }
    }
}
