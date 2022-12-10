using System;

namespace Tetrifact.Core
{
    public class FatalException : Exception
    {
        public FatalException(string message, Exception ex) : base(message, ex) 
        { 

        }
    }
}
