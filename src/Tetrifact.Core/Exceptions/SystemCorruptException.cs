using System;

namespace Tetrifact.Core
{
    public class SystemCorruptException : Exception
    {
        public SystemCorruptException(string message) : base($"Repository file structure is fatally corrupted - {message}")
        {

        }
    }
}
