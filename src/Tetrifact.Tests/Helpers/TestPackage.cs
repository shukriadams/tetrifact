namespace Tetrifact.Tests
{
    /// <summary>
    /// Test package with minimal data - has only 1 file
    /// </summary>
    public class TestPackage
    {
        /// <summary>
        /// Content of single file in package
        /// </summary>
        public byte[] Content;

        /// <summary>
        /// Path of single file in package
        /// </summary>
        public string Path;

        /// <summary>
        /// Hash of the single file in package
        /// </summary>
        public string Hash;

        /// <summary>
        /// Name of package
        /// </summary>
        public string Id;
    }
}
