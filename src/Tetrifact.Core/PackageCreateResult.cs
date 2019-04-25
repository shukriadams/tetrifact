namespace Tetrifact.Core
{
    public class PackageCreateResult
    {
        public bool Success { get; set; }
        public string PackageHash { get; set; }
        public string PublicError { get; set; }
        public PackageCreateErrorTypes? ErrorType { get; set; }
    }
}
