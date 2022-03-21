using System;
using System.Linq;

namespace Tetrifact.Core
{
    public class FileHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double BytesToMegabytes(long bytes)
        {
            var megs = (bytes / 1024f) / 1024f;
            return Math.Round(megs, 0);
        }


        public static string ToUnixPath(string path)
        {
            return path.Replace("\\", "/");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RemoveFirstDirectoryFromPath(string path)
        {
            // convert to unix format
            path = ToUnixPath(path);

            string[] items = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (items.Length == 1)
                return path;

            return string.Join("/", items.Skip(1));
        }
    }
}


