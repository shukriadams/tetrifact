using System;

namespace Tetrifact.Core
{
    public interface IDiffService
    {
        DateTime? LastRun { get; }

        void Start();

        void Stop();
    }
}
