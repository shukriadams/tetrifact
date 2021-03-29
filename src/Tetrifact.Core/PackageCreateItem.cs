using System.IO;

namespace Tetrifact.Core
{
    public class PackageCreateItem
    {
        public string FileName { get; set; }
        public Stream Content { get; set; }

        public PackageCreateItem()
        { }

        public PackageCreateItem(Stream content, string filename)
        {
            this.FileName = filename;
            this.Content = content;
        }
    }
}