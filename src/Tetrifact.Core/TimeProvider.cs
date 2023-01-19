using System;

namespace Tetrifact.Core
{
    public class TimeProvider : ITimeProvideer
    {
        public DateTime GetUtcNow() 
        {
            return DateTime.UtcNow;
        }
    }
}
