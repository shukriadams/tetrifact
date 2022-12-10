using System;

namespace Tetrifact.Core
{
    public class MetricsStaleException : Exception
    {
        public MetricsStaleException(string message) : base (message)
        {

        }
    }
}
