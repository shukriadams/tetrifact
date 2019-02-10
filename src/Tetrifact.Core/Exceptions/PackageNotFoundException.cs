using System;

namespace Tetrifact.Core
{
    public class PackageNotFoundException : Exception
    {
        public string PackageId { get; private set; }

        public PackageNotFoundException(string packageId)
        {
            this.PackageId = packageId;
        }
    }
}
