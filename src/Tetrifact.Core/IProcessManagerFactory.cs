namespace Tetrifact.Core
{
    public interface IProcessManagerFactory
    {
        IProcessManager GetInstance(ProcessManagerContext key);
        void SetInstance(ProcessManagerContext key, IProcessManager instance);
        void ClearExpired();
    }
}
