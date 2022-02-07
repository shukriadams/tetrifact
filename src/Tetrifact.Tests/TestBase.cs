using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ninject;
using System.Reflection;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public abstract class TestBase
    {
        protected StandardKernel Kernel;
        protected ISettings Settings;

        /// <summary>
        /// Common method for convert JsonResult from API endjoing to dynamic object
        /// </summary>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        public dynamic ToDynamic(ActionResult actionResult)
        {
            string jrawJson;
            if (actionResult is NotFoundObjectResult)
            {
                NotFoundObjectResult notFound = actionResult as NotFoundObjectResult;
                jrawJson = JsonConvert.SerializeObject(notFound.Value);
            } 
            else 
            {
                JsonResult jsonResult = actionResult as JsonResult;
                jrawJson = JsonConvert.SerializeObject(jsonResult.Value);
            }

            dynamic obj = JsonConvert.DeserializeObject(jrawJson);
            return obj;
        }

        public TestBase()
        {
            this.Kernel = new StandardKernel();
            this.Kernel.Load(Assembly.GetExecutingAssembly());
            this.Settings = Kernel.Get<ISettings>();
        }
    }
}
