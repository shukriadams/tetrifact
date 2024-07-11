using System;

namespace Tetrifact.Core
{
    public class InvalidDiffComparison : Exception
    {
        public InvalidDiffComparison(string message) : base(message)
        { 

        }
    }
}
