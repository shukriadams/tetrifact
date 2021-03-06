﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ninject;
using System.Reflection;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public abstract class TestBase
    {
        protected StandardKernel Kernel;
        protected ITetriSettings Settings;

        /// <summary>
        /// Common method for convert JsonResult from API endjoing to dynamic object
        /// </summary>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        public dynamic ToDynamic(ActionResult actionResult)
        {
            JsonResult jsonResult = (JsonResult)actionResult;
            string jrawJson = JsonConvert.SerializeObject(jsonResult.Value);
            dynamic obj = JsonConvert.DeserializeObject(jrawJson);
            return obj;
        }

        public TestBase()
        {
            this.Kernel = new StandardKernel();
            this.Kernel.Load(Assembly.GetExecutingAssembly());
            this.Settings = Kernel.Get<ITetriSettings>();
        }
    }
}
