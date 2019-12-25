using System.IO;

namespace Tetrifact.Core
{
    /// <summary>
    /// Wraps response when a single file is requested.
    /// </summary>
    public class GetFileResponse
    {
        public string FileName { get; set; }
        public Stream Content { get; set; }

        public GetFileResponse(Stream content, string filename)
        {
            this.Content = content;
            this.FileName = filename;
        }
    }
}
