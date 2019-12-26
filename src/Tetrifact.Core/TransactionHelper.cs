using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    public class TransactionHelper
    {
        private readonly IIndexReader _indexReader;
        
        private readonly ISettings _settings;

        public TransactionHelper(IIndexReader indexReader, ISettings settings) 
        {
            _indexReader = indexReader;
            _settings = settings;
        }

        public ProjectView GetProjectView(string project) 
        {
            // get a list of the transacations we want to keep (our history)
            IList<DirectoryInfo> transacationHistoryToKeep = _indexReader.GetLatestTransactionsInfo(project, _settings.TransactionHistoryDepth).ToList();

            // shorten the history to make it easier to do .include string checks
            IEnumerable<string> shortenedTransactionHistory = transacationHistoryToKeep.Select(r => r.Name);

            // list of shards and manifests that make up history
            List<string> shardsInHistory = new List<string>();
            List<string> manifestsInHistory = new List<string>();

            // extract all pointers from transactions in history  
            foreach (DirectoryInfo currentTransaction in transacationHistoryToKeep)
            {
                IEnumerable<FileInfo> files = currentTransaction.GetFiles();
                foreach (FileInfo file in files)
                {
                    string pointer = File.ReadAllText(file.FullName);
                    if (file.Name.EndsWith("_manifest"))
                        manifestsInHistory.Add(pointer);
                    else if (file.Name.EndsWith("_shard"))
                        shardsInHistory.Add(pointer);
                }
            }

            return new ProjectView(manifestsInHistory, shortenedTransactionHistory, shardsInHistory);
        }
    }
}
