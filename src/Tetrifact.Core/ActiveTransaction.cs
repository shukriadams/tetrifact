using System;
using System.IO;

namespace Tetrifact.Core
{
    /// <summary>
    /// Wraps a DirectoryInfo object that points to the folder containing the active (most recent) transaction at the time
    /// of querying the transaction table. Creating ActiveTransaction automatically locks the transaction folder so it will 
    /// not be cleaned up while being used.
    /// </summary>
    public class ActiveTransaction
    {
        public DirectoryInfo Info {get; private set;}

        FileStream patchStream;

        string _lockfile;

        public ActiveTransaction(DirectoryInfo info)
        {
            Info = info;
            try 
            {
                // rty
                _lockfile = Path.Join(info.FullName, $"{Constants.LockFragment}{Guid.NewGuid()}");
                patchStream = new FileStream(_lockfile, FileMode.Create, FileAccess.Write);
            }
            catch(IOException) 
            {
                throw new MissingTransacationException();
            }
        }

        public void Unlock()
        {
            patchStream.Close();
            File.Delete(_lockfile);
        }
    }
}
