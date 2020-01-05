using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tetrifact.Core;

namespace Tetrifact.Dev
{
    public class FormFileHelper
    {
        /// <summary>
        /// Creates a FormFile collection with a single file
        /// </summary>
        /// <param name="content"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IList<PackageCreateItem> Single(string content, string path) 
        {
            Stream file = StreamsHelper.StreamFromBytes(Encoding.ASCII.GetBytes(content));
            return new List<PackageCreateItem>() { new PackageCreateItem(file, path) };
        }

        /// <summary>
        /// Creates a single-item FormFile collection from a single stream. Use this for archive uploads.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IList<PackageCreateItem> FromStream(Stream content, string name)
        {
            return new List<PackageCreateItem>() { new PackageCreateItem(content, name) };
        }

        /// <summary>
        /// Creates a collection of fileform items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IList<PackageCreateItem> Multiple(IEnumerable<DummyFile> items)
        {
            IList<PackageCreateItem> files = new List<PackageCreateItem>();
            foreach (DummyFile item in items) 
            {
                Stream file = StreamsHelper.StreamFromBytes(Encoding.ASCII.GetBytes(item.Content));
                files.Add( new PackageCreateItem(file, item.Path) );
            }

            return files;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static string GetHash(IEnumerable<DummyFile> files)
        {
            StringBuilder hashes = new StringBuilder();
            files = files.OrderBy(r => r.Path);
            foreach (DummyFile file in files)
            {
                hashes.Append(Core.HashService.FromString(file.Path));
                hashes.Append(Core.HashService.FromByteArray(file.Data));
            }

            return Core.HashService.FromString(hashes.ToString());
        }
    }
}
