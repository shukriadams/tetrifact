using System;

namespace Tetrifact.Core
{
    public interface ITimeProvider
    {
        DateTime GetUtcNow();
    }
}
