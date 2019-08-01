using System;

namespace Tetrifact.Core
{
    public class DiskUseStats
    {
        public long TotalBytes { get; set ;}
        public long FreeBytes { get; set ;}

        public int ToPercent ()
        {
            if (this.TotalBytes == 0)
                throw new Exception("Total bytes has not been set yet");

            return (int)((100 * this.FreeBytes) / this.TotalBytes);
        }
    }
}
