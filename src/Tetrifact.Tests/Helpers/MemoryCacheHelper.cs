using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Tetrifact;
using Tetrifact.Tests;

public class MemoryCacheHelper
{
    public static IMemoryCache GetInstance(){
        ServiceCollection services = new ServiceCollection();
        services.AddMemoryCache();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetService<IMemoryCache>();
    }

    /// <summary>
    /// todo : this should be moved to IOC
    /// </summary>
    /// <returns></returns>
    public static ITetrifactMemoryCache GetTetrifactMemoryCacheInstance()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddMemoryCache();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return new TetrifactTestMemoryCache(serviceProvider.GetService<IMemoryCache>());
    }
}