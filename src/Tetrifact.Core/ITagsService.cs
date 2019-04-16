using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface ITagsService
    {
        void AddTag(string packageId, string tag);

        void RemoveTag(string packageId, string tag);

        IEnumerable<string> ReadTagsFromIndex();

        IEnumerable<string> GetPackageIdsWithTag(string tag);
    }
}
