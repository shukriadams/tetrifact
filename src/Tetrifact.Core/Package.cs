using System;

namespace Tetrifact.Core
{
    /// <summary>
    /// Minimal version of Manifest with info for list display
    /// </summary>
    public class Package
    {
        public string Id { get; set; }

        public DateTime CreatedUtc { get; set; }
    }
}
