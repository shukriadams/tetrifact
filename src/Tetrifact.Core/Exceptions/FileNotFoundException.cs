using System;

namespace Tetrifact.Core
{
    public class FileNotFoundException : Exception
    {
        public string File { get; private set; }

        public FileNotFoundException(string file)
        {
            this.File = file;
        }
    }
}
