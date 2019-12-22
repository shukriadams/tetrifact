using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class ArchiveHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static Stream ZipStreamFromFiles(IEnumerable<DummyFile> files) 
        {
            MemoryStream memoryStream = new MemoryStream();

            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (DummyFile file in files) 
                {
                    ZipArchiveEntry archiveFile = archive.CreateEntry(file.Path);
                    using (Stream entryStream = archiveFile.Open())
                    using (BinaryWriter streamWriter = new BinaryWriter(entryStream))
                    {
                        streamWriter.Write(file.Data, 0, file.Data.Length);
                    }
                }
            }

            return memoryStream;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipStream"></param>
        /// <returns></returns>
        public static IEnumerable<DummyFile> FilesFromZipStream(Stream zipStream) 
        {
            try
            {
                List<DummyFile> files = new List<DummyFile>();

                using (var archive = new ZipArchive(zipStream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry != null)
                        {
                            using (Stream unzippedEntryStream = entry.Open())
                            {
                                files.Add(new DummyFile {
                                    Data = StreamsHelper.StreamToByteArray(unzippedEntryStream),
                                    Path = entry.FullName
                                });
                                
                            }
                        }
                    }
                }

                return files;
            }
            finally 
            {
                zipStream.Close();
            }
        }
    }
}
