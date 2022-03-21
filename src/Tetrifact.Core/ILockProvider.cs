namespace Tetrifact.Core
{
    public interface ILockProvider
    {
        ILock Instance {  get; }
        void Reset();
    }
}
