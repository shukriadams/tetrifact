using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Tetrifact.Tests 
{
    public class MemoryCacheHelper
    {
        public static IMemoryCache GetInstance()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddMemoryCache();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IMemoryCache>();
        }
    }
}
