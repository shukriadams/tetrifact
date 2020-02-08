using System;

namespace Tetrifact.Core
{
    public class PackageCreateException : Exception
    {
        public PackageCreateErrorTypes ErrorType { get; set; }

        public string PublicError { get; set; }

        public PackageCreateException() 
        {
            PublicError = string.Empty;
        }
    }
}
