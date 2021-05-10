using System.IO;

namespace Tetrifact.Core
{
    /// <summary>
    /// Wraps uploaded file content when creating package
    /// </summary>
    public class PackageCreateItem
    {
        #region FIELDS

        public string FileName { get; set; }

        public Stream Content { get; set; }

        #endregion

        #region CTORS

        public PackageCreateItem()
        { }

        public PackageCreateItem(Stream content, string filename)
        {
            this.FileName = filename;
            this.Content = content;
        }

        #endregion
    }
}