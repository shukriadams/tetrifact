using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

public class MemoryCacheHelper
{
    public static IMemoryCache GetInstance(){
        ServiceCollection services = new ServiceCollection();
        services.AddMemoryCache();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetService<IMemoryCache>();
    }
}