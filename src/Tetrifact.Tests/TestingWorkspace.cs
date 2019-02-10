using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class TestingWorkspace : IWorkspace
    {
        private Manifest _manifest = new Manifest();

        public Dictionary<string, byte[]> Incoming = new Dictionary<string, byte[]>();

        public Dictionary<string, byte[]> Repository = new Dictionary<string, byte[]>();

        public Manifest Manifest { get { return _manifest; } }

        /// <summary>
        /// Return empty string, this implementation has no file system presence.
        /// </summary>
        public string WorkspacePath { get { return string.Empty; } }

        async public Task<bool> AddIncomingFileAsync(Stream fileStream, string relativePath)
        {
            if (fileStream.Length == 0)
                return false;

            using (var stream = new MemoryStream())
            {
                await fileStream.CopyToAsync(stream);
                this.Incoming.Add(relativePath, stream.ToArray());
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
                            this.Incoming.Add(entry.FullName, ms.ToArray());
                        }
                    }

                }
            }
        }

        public IEnumerable<string> GetIncomingFileNames()
        {
            return this.Incoming.Select(r => r.Key);
        }

        public void WriteFile(string fileInIncoming, string hash, string packageId)
        {
            // move file to public folder
            this.Repository.Add(fileInIncoming, this.Incoming[fileInIncoming]);
            this.Incoming.Remove(fileInIncoming);
            this.Manifest.Files.Add(new ManifestItem { Path = fileInIncoming, Hash = hash });
        }

        public void WriteManifest(string packageId, string combinedHash)
        {
            // calculate package hash from child hashes
            this.Manifest.Hash = combinedHash;
        }

        public string GetIncomingFileHash(string path)
        {
            byte[] content = this.Incoming[path];
            return HashService.FromByteArray(content);
        }

    }
}
