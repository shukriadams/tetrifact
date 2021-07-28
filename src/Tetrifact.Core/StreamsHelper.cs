using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Tetrifact.Core
{
    public class StreamsHelper
    {
        /// <summary>
        /// Converts a string to stream.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MemoryStream StreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }
    
        /// <summary>
        /// Generates a stream from a byte array
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MemoryStream StreamFromBytes(byte[] value)
        {
            return new MemoryStream(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamToByteArray(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (stream)
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Converts a stream content to string.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string StreamToString(Stream stream)
        {
            return Encoding.Default.GetString(StreamToByteArray(stream));
        }

        /// <summary>
        /// Converts item a archive stream to a dictionary, where keys are file paths of items in archive stream.
        /// </summary>
        /// <param name="archiveStream"></param>
        /// <returns></returns>
        public static Dictionary<string, byte[]> ArchiveStreamToCollection(Stream archiveStream)
        {
            Dictionary<string, byte[]> items = new Dictionary<string, byte[]>();

            using (var archive = new ZipArchive(archiveStream))
            {
                foreach (ZipArchiveEntry entry in archive.Entries.Where(entry => entry != null))
                {
                    using (Stream unzipStream = entry.Open())
                    {
                        items.Add(entry.FullName, StreamToByteArray(unzipStream));
                    }
                }
            }

            return items;
        }
    }
}
