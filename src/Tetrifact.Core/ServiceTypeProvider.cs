using Microsoft.Extensions.DependencyInjection;
using System;

namespace Tetrifact.Core
{
    public class ServiceTypeProvider : ITypeProvider
    {
        private IServiceProvider _serviceProvider;

        public ServiceTypeProvider(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
        }

        public T GetInstance<T>() 
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                T instance = (T)scope.ServiceProvider.GetRequiredService(typeof(T));
                return instance;
            }
        }
    }
}
