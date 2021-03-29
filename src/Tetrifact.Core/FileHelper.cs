using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tetrifact.Core
{
    public class FileHelper
    {
        public static double BytesToMegabytes(long bytes)
        {
            var megs = (bytes / 1024f) / 1024f;
            return Math.Round(megs, 0);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RemoveFirstDirectoryFromPath(string path)
        {
            path = path.Replace("\\", "/");
            string[] items = path.Split("/");
            if (items.Length == 1)
                return path;

            return string.Join("/", items.Skip(1));
        }


        public static DiskUseStats GetDiskUseSats()
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DriveInfo drive = new DriveInfo(path);
            DiskUseStats stats = new DiskUseStats();
            
            stats.TotalBytes = drive.TotalSize;
            stats.FreeBytes = drive.AvailableFreeSpace;

            return stats; 
        }
    }
}


