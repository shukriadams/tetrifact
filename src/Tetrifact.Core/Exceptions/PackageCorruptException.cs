using System;

namespace Tetrifact.Core
{
    public class PackageCorruptException : Exception
    {
        public PackageCorruptException(string message) : base (message)
        {

        }

        public PackageCorruptException(string message, Exception exception) : base(message, exception)
        {

        }
    }
}
