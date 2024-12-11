using System;

namespace Tetrifact.Core
{
    public class Global
    {
        public static DateTime StartTimeUtc { get; private set; } = DateTime.UtcNow;
    }
}
