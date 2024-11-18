using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class ProcessInfo
    {
        public string Identifier { get ;set; }

        public DateTime Started { get; set; }

        public DateTime SignOfLife { get; set; }

    }

    public class CurrentProcesses
    {
        private IList<ProcessInfo> _packagesArriving = new List<ProcessInfo>();
        
        private IList<ProcessInfo> _archivesGenerating = new List<ProcessInfo>();
    
    }
}
