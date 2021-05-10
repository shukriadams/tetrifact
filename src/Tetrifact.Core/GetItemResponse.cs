using System.IO;

namespace Tetrifact.Core
{
    /// <summary>
    /// Wraps multiple objects retrieved when querying an Index for a file.
    /// </summary>
    public class GetFileResponse
    {
        #region PROPERTIES

        /// <summary>
        /// Relative path and filename of file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Stream with file content.
        /// </summary>
        public Stream Content { get; set; }

        #endregion

        #region CTORS

        public GetFileResponse(Stream content, string filename)
        {
            this.Content = content;
            this.FileName = filename;
        }

        #endregion
    }
}
