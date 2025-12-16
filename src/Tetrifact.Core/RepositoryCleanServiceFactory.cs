namespace Tetrifact.Core;

public class RepositoryCleanServiceFactory : IRepositoryCleanServiceFactory
{
    /// <summary>
    /// Calls out to some provider to give us an instance of some type. We do it this way to let us use multiple IOC providers to instantiate
    /// this type.
    /// </summary>
    /// <returns></returns>
    public delegate IRepositoryCleanService CreateInstance();

    private readonly CreateInstance _createInstance;
    
    public RepositoryCleanServiceFactory(CreateInstance create)
    {
        _createInstance = create;
    }

    public IRepositoryCleanService Create()
    {
        return _createInstance();
    }
}