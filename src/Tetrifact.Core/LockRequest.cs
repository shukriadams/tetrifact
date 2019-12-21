using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public class LockRequest
    {
        private TaskCompletionSource<object> _done;

        public async Task Get()
        {
            _done = new TaskCompletionSource<object>();

            Task.Factory.StartNew(delegate()
            {
                LinkLock.Instance.WaitUntilClear();
                _done.TrySetResult(null);
            });

            await _done.Task;
        }

    }
}
