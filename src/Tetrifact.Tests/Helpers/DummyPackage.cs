using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Holds data for a test package
    /// </summary>
    public class DummyPackage
    {
        public IList<DummyFormFile> Files { get; private set; }

        /// <summary>
        /// Name of the package
        /// </summary>
        public string Id;

        public DummyPackage() 
        {
            this.Files = new List<DummyFormFile>();
        }

        public DummyPackage(string name, IEnumerable<DummyFormFile> files)
        {
            this.Id = name;
            this.Files = new List<DummyFormFile>();
            this.Files = files.ToList();
        }
    }
}
