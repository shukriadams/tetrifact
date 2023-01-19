using System;

namespace Tetrifact.Core
{
    public interface ITimeProvideer
    {
        DateTime GetUtcNow();
    }
}
