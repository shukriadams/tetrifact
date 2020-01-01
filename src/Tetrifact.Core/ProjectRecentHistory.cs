using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Summary of a project, containing latest values. Used to clean a project out ; anything not in the state view will be deleted
    /// </summary>
    public class ProjectRecentHistory
    {
        public IEnumerable<string> Manifests { get; private set; }
        public IEnumerable<string> Transactions { get; private set; }
        public IEnumerable<string> Shards { get; private set; }

        public ProjectRecentHistory(IEnumerable<string> manifests, IEnumerable<string> transactions, IEnumerable<string> shards) 
        {
            this.Manifests = manifests;
            this.Transactions = transactions;
            this.Shards = shards;
        }
    }
}
