using System;

namespace Tetrifact.Core
{
    public class ProjectNotFoundException : Exception
    {
        public string Project { get; private set; }

        public ProjectNotFoundException(string project)
        {
            this.Project = project;
        }
    }
}
