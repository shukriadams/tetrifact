using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Minimal version of Manifest with info for list display
    /// </summary>
    public class Package
    {
        public string Id { get; set; }

        public DateTime CreatedUtc { get; set; }

        public string Description { get; set; }

        public string Hash { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
