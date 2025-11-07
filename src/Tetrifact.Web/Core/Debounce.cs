using System;

namespace Tetrifact.Web
{
    public class Debounce
    {
        public delegate void Do();
        private DateTime? _lastcalled;
        private readonly TimeSpan _wait;
        private readonly Do _work;
        private bool _busy;

        public Debounce(TimeSpan wait, Do work)
        {
            _wait = wait;
            _work = work;
        }

        public void Invoke() 
        {
            try 
            {
                if (_busy)
                    return;

                if (_lastcalled != null && (DateTime.Now - _lastcalled.Value).Ticks < _wait.Ticks)
                    return;

                _busy = true;
                _work.Invoke();
                _lastcalled = DateTime.Now;
            }
            finally 
            { 
                _busy = false; 
            }
        }

    }
}
