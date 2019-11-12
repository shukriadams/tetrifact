using System;

namespace Tetrifact.Core
{
    public class MissingProjectStructure : Exception
    {
        public string Project { get; private set; }

        public MissingProjectStructure(string project)
        {
            this.Project = project;
        }
    }
}
