using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    public class LocalStorageService : IStorageService
    {
        private readonly ISettings _settings;

        public LocalStorageService(ISettings settings) 
        {
            _settings = settings;
        }

        public IEnumerable<string> GetExpiredtArchivePaths()
        {
            DirectoryInfo info = new DirectoryInfo(_settings.ArchivePath);

            // get all existing archives, sorted by create date
            IEnumerable<FileInfo> files = info.GetFiles()
                .OrderByDescending(p => p.CreationTime)
                .Skip(_settings.MaximumArchivesToKeep);

            return files.Select(f => f.FullName);
        }

    }
}
