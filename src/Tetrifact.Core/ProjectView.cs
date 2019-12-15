using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class ProjectView
    {
        public IEnumerable<string> Manifests { get; private set; }
        public IEnumerable<string> Transactions { get; private set; }
        public IEnumerable<string> Shards { get; private set; }

        public ProjectView(IEnumerable<string> manifests, IEnumerable<string> transactions, IEnumerable<string> shards) 
        {
            this.Manifests = manifests;
            this.Transactions = transactions;
            this.Shards = shards;
        }
    }
}
