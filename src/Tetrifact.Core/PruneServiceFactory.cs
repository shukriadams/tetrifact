namespace Tetrifact.Core;

public class PruneServiceFactory : IPruneServiceFactory
{
    /// <summary>
    /// Calls out to some provider to give us an instance of some type. We do it this way to let us use multiple IOC providers to instantiate
    /// this type.
    /// </summary>
    /// <returns></returns>
    public delegate IPruneService CreateInstance();

    private readonly CreateInstance _createInstance;
    
    public PruneServiceFactory(CreateInstance create)
    {
        _createInstance = create;
    }

    public IPruneService Create()
    {
        return _createInstance();
    }
}