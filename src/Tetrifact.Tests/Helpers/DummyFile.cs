namespace Tetrifact.Tests
{
    /// <summary>
    /// Data type corresponding to a file. Used to create dummy projects.
    /// </summary>
    public class DummyFile
    {
        /// <summary>
        /// TODO : GET RID OF STRING MEMBER, A dummy file should contain either string or byte content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// A dummy file should contain either string or byte content
        /// </summary>
        public byte[] Data { get; set; } 

        public string Path { get; set; }
    }
}
