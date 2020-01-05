namespace Tetrifact.Dev
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

        public DummyFile() 
        {

        }

        public DummyFile(byte[] data, string path) 
        {
            this.Data = data;
            this.Path = path;
        }


        public DummyFile(string content, string path)
        {
            this.Content = content;
            this.Path = path;
        }
    }
}
