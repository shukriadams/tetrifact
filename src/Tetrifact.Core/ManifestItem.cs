namespace Tetrifact.Core
{
    /// <summary>
    /// File in a manifest
    /// </summary>
    public class ManifestItem : IPackageFile
    {
        /// <summary>
        /// Relative path of file in manifest
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Hash of the file content.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Cloaked path+hash of file. This is the file's public unique id.
        /// </summary>
        public string Id { get; set; }
    }
}
