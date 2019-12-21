using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

public class MemoryCacheHelper
{
    /// <summary>
    /// Creates an instance of a memory cache - used for tests which require in-memmory caching.
    /// </summary>
    /// <returns></returns>
    public static IMemoryCache GetInstance(){
        ServiceCollection services = new ServiceCollection();
        services.AddMemoryCache();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetService<IMemoryCache>();
    }
}