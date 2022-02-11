using System;

namespace Tetrifact.Core
{
    /// <summary>
    /// Thrown whene the user attempts an operation that would normally be valid but is not permitted due to setup.
    /// </summary>
    public class OperationNowAllowedException : Exception
    {

    }
}
