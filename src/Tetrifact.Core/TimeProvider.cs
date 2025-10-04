using System;

namespace Tetrifact.Core
{
    public class TimeProvider : ITimeProvider
    {
        public virtual DateTime GetUtcNow() 
        {
            return DateTime.UtcNow;
        }
    }
}
