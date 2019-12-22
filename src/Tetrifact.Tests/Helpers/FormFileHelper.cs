using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class FormFileHelper
    {
        /// <summary>
        /// Creates a FormFile collection with a single file
        /// </summary>
        /// <param name="content"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IList<IFormFile> Single(string content, string path) 
        {
            Stream file = StreamsHelper.StreamFromBytes(Encoding.ASCII.GetBytes(content));
            return new List<IFormFile>() { new FormFile(file, 0, file.Length, "Files", path) };
        }

        /// <summary>
        /// Creates a single-item FormFile collection from a single stream. Use this for archive uploads.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IList<IFormFile> FromStream(Stream content, string name)
        {
            return new List<IFormFile>() { new FormFile(content, 0, content.Length, "Files", name) };
        }

        /// <summary>
        /// Creates a collection of fileform items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IList<IFormFile> Multiple(IEnumerable<DummyFile> items)
        {
            IList<IFormFile> files = new List<IFormFile>();
            foreach (DummyFile item in items) 
            {
                Stream file = StreamsHelper.StreamFromBytes(Encoding.ASCII.GetBytes(item.Content));
                files.Add( new FormFile(file, 0, file.Length, "Files", item.Path) );
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
