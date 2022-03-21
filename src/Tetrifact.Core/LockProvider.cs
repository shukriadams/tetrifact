namespace Tetrifact.Core
{
    public class LockProvider : ILockProvider
    {
        private static ILock _instance;

        public ILock Instance { get { return _instance; } }

        public LockProvider()
        { 

        }

        /// <summary>
        /// Private constructor, ensures instance
        /// </summary>
        static LockProvider()
        {
            _instance = new ProcessLock();
        }

        /// <summary>
        /// This is for testing purposes only!
        /// </summary>
        public void Reset()
        {
            _instance = new ProcessLock();
        }
    }
}
