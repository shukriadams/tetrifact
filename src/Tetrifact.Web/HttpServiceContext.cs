using Microsoft.AspNetCore.Http;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class HttpServiceContext : ITypeProvider
    {
        IHttpContextAccessor _accessor;

        public HttpServiceContext(IHttpContextAccessor accessor) 
        {
            _accessor = accessor;
        }

        public T GetInstance<T>()
        {
            return (T)_accessor.HttpContext.RequestServices.GetService(typeof(T));
        }
    }
}
