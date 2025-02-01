using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Type that provides information about content on current storage implementation, egs, local filessyste, or object storage.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Gets absolute paths of archives that exceed keep policy.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetExpiredtArchivePaths();
    }
}
