using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class TestingWorkspace : IWorkspace
    {
        private static Manifest _manifest = new Manifest();

        public static Dictionary<string, byte[]> Incoming = new Dictionary<string, byte[]>();

        public static Dictionary<string, byte[]> Repository = new Dictionary<string, byte[]>();

        public Manifest Manifest { get { return _manifest; } }

        /// <summary>
        /// For testing purposes. Wipes contents of this workspace
        /// </summary>
        public static void Reset()
        {
            Incoming = new Dictionary<string, byte[]>();
            Repository = new Dictionary<string, byte[]>();
            _manifest = new Manifest();
        }

        /// <summary>
        /// Return empty string, this implementation has no file system presence.
        /// </summary>
        public string WorkspacePath { get { return string.Empty; } }

        public bool AddIncomingFile(Stream fileStream, string relativePath)
        {
            if (fileStream.Length == 0)
                return false;

            using (var stream = new MemoryStream())
            {
                fileStream.CopyTo(stream);
                Incoming.Add(relativePath, stream.ToArray());
                return true;
            }
        }

        public void AddArchiveContent(Stream file)
        {
            using (ZipArchive archive = new ZipArchive(file))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry == null)
                        continue;

                    using (Stream unzippedEntryStream = entry.Open())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            unzippedEntryStream.CopyTo(ms);
                            byte[] unzippedArray = ms.ToArray();
                            Incoming.Add(entry.FullName, ms.ToArray());
                        }
                    }

                }
            }
        }

        public IEnumerable<string> GetIncomingFileNames()
        {
            return Incoming.Select(r => r.Key);
        }

        public void WriteFile(string fileInIncoming, string hash, string packageId)
        {
            // move file to public folder
            Repository.Add(fileInIncoming, Incoming[fileInIncoming]);
            Incoming.Remove(fileInIncoming);
            this.Manifest.Files.Add(new ManifestItem { Path = fileInIncoming, Hash = hash });
        }

        public void WriteManifest(string packageId, string combinedHash)
        {
            // calculate package hash from child hashes
            this.Manifest.Hash = combinedHash;
        }

        public string GetIncomingFileHash(string path)
        {
            byte[] content = Incoming[path];
            return HashService.FromByteArray(content);
        }

        public void Dispose()
        {

        }
    }
}
