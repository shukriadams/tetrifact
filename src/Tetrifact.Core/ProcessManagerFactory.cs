using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Factory for static instances of IProcessManagers. Instances can be injected if needed.
    /// </summary>
    public class ProcessManagerFactory : IProcessManagerFactory
    {
        /// <summary>
        /// Calls out to some provider to give us an instance of IProcessManager. We do it this way to let us use multiple IOC providers to instantiate
        /// this type.
        /// </summary>
        /// <returns></returns>
        public delegate IProcessManager CreateInstance();

        #region FIELDS

        private readonly CreateInstance _createInstance;

        private Dictionary<ProcessManagerContext, IProcessManager> _instances = new Dictionary<ProcessManagerContext, IProcessManager>();

        #endregion

        #region CTORS

        public ProcessManagerFactory(CreateInstance create)
        {
            _createInstance = create;
        }

        #endregion

        #region METHODS

        public IProcessManager GetInstance(ProcessManagerContext key) 
        {
            if (!_instances.ContainsKey(key))
            {
                lock (_instances) 
                {
                    IProcessManager instance = _createInstance();
                    instance.Context = key.ToString();
                    _instances[key] = instance;
                }
            }

            return _instances[key];
        }

        public void SetInstance(ProcessManagerContext key, IProcessManager instance)
        {
            lock (_instances) 
            {
                if (_instances.ContainsKey(key))
                {
                    _instances.Remove(key);
                    _instances[key] = instance;
                }
            }
        }

        public void ClearExpired() 
        {
            lock (_instances) 
            {
                foreach (IProcessManager instance in _instances.Values)
                    instance.ClearExpired();
            }
        }

        #endregion
    }
}
