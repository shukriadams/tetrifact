using System;

namespace Tetrifact.Core
{
    public class ProjectCorruptException : Exception
    {
        public ProjectCorruptException(string project, string message) : base($"File structure for project \"{project}\" is fatally corrupted - {message}")
        {

        }
    }
}
