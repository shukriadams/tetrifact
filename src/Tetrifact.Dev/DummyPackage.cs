using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Dev
{
    /// <summary>
    /// Holds data for a test package
    /// </summary>
    public class DummyPackage
    {
        public IList<DummyFile> Files { get; private set; }

        /// <summary>
        /// Name of the package
        /// </summary>
        public string Id;

        public DummyPackage() 
        {
            this.Files = new List<DummyFile>();
        }

        public DummyPackage(string name, IEnumerable<DummyFile> files)
        {
            this.Id = name;
            this.Files = new List<DummyFile>();
            this.Files = files.ToList();
        }
    }
}
