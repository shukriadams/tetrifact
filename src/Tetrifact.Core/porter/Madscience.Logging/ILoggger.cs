//PORTER-WRAPPER!
namespace Tetrifact.Core.Porter_Packages {
//PORTER-WRAPPER!


using System;

namespace Madscience.Loggger
{
    public interface ILoggger : IDisposable
    {
        int VerbosityThreshold { get; set; }

        void Dispose();

        void Error(object source, string message); 

        void Error(object source, object exception);

        void Error(object source, string message, object exception);

        void Warn(object source, string message);

        void Warn(object source, object exception);

        void Warn(object source, string message, object exception);

        void Status(object source, string message, int verbosity = 0);

        void Status(string source, string message, int verbosity = 0);

        void Debug(object source, string message, int verbosity = 0);

        void Debug(string source, string message, int verbosity = 0);
        
        void Trace(object source, string message, int verbosity = 0);

        void Trace(string source, string message, int verbosity);
    }
}


//PORTER-WRAPPER!
}
//PORTER-WRAPPER!