namespace Tetrifact.Core
{
    public interface ITypeProvider
    {
        T GetInstance<T>();
    }
}
