namespace Tetrifact.Core
{
    public class PackageAddResult
    {
        public bool Success { get; set; }
        public string PackageHash { get; set; }
        public string PublicError { get; set; }
        public PackageAddErrorTypes? ErrorType { get; set; }
    }
}
