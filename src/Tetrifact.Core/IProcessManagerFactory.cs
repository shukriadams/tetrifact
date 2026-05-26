using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface IProcessManagerFactory
    {
        IProcessManager GetInstance(ProcessManagerContext key);
        IEnumerable<IProcessManager> GetInstances();
        void SetInstance(ProcessManagerContext key, IProcessManager instance);
        void ClearExpired();
    }
}
