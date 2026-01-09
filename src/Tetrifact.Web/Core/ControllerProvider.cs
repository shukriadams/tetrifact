using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Tetrifact.Web.Porter_Packages.MadScience_SimpleDI;

namespace Tetrifact.Web
{
    /// <summary>
    ///  
    /// </summary>
    public class ControllerProvider : IControllerActivator
    {
        public object Create(ControllerContext controllerContext)
        {
            if (controllerContext == null)
                throw new ArgumentNullException(nameof(controllerContext));

            if (controllerContext.ActionDescriptor == null)
                throw new ArgumentException(nameof(controllerContext.ActionDescriptor));

            var controllerTypeInfo = controllerContext.ActionDescriptor.ControllerTypeInfo;

            if (controllerTypeInfo == null)
                throw new ArgumentException(nameof(controllerContext.ActionDescriptor.ControllerTypeInfo));
            
            SimpleDI simpleDi = new SimpleDI();
            return simpleDi.Resolve(controllerTypeInfo.UnderlyingSystemType);
        }


        
        public void Release(ControllerContext context, object controller)
        {
            if (controller is IDisposable disposable)
                disposable.Dispose();
        }
    }

}