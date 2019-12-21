using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Collections.Generic;
using System.IO;
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
        /// Creates a collection of fileform items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IList<IFormFile> Multiple(IEnumerable<DummyFormFile> items)
        {
            IList<IFormFile> files = new List<IFormFile>();
            foreach (DummyFormFile item in items) 
            {
                Stream file = StreamsHelper.StreamFromBytes(Encoding.ASCII.GetBytes(item.Content));
                files.Add( new FormFile(file, 0, file.Length, "Files", item.Path) );
            }

            return files;
        }
    }
}
