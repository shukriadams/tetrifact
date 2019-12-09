using System;
using System.IO;

namespace Tetrifact.Core
{
    public class Transaction
    {
        private readonly string _finalTransactionFolder;
        private readonly string _tempTransactionFolder;
        private readonly IIndexReader _indexReader;

        public Transaction(ITetriSettings settings, IIndexReader indexReader, string project) 
        {
            _indexReader = indexReader;

            long ticks = DateTime.UtcNow.Ticks;
            _tempTransactionFolder = Path.Combine(settings.ProjectsPath, project, Constants.TransactionsFragment, $"~{ticks}");
            _finalTransactionFolder = Path.Combine(settings.ProjectsPath, project, Constants.TransactionsFragment, $"{ticks}");
            Directory.CreateDirectory(_tempTransactionFolder);

            // find current transaction and copy it's contents
            DirectoryInfo activeTransaction = _indexReader.GetActiveTransactionInfo(project);
            if (activeTransaction != null) 
            {
                foreach (FileInfo file in activeTransaction.GetFiles())
                    file.CopyTo(Path.Combine(_tempTransactionFolder, file.Name));
            }
        }

        public void SetHead(string package) 
        {
            File.WriteAllText(Path.Combine(_tempTransactionFolder, "head"), package);
        }

        public void AddManifestPointer(string package, string targetName) 
        {
            File.WriteAllText(Path.Combine(_tempTransactionFolder, $"{package}_manifest"), targetName);
        }

        public void AddShardPointer(string package, string targetName) 
        {
            File.WriteAllText(Path.Combine(_tempTransactionFolder, $"{package}_shard"), targetName);
        }

        public void RemoveManifestPointer(string package)
        {
            string path = Path.Combine(_tempTransactionFolder, $"{package}_manifest");
            if (File.Exists(path))
                File.Delete(path);
        }

        public void RemoveShardPointer(string package)
        {
            string path = Path.Combine(_tempTransactionFolder, $"{package}_shard");
            if (File.Exists(path))
                File.Delete(path);
        }

        public void Commit() 
        {
            // flip transaction live
            Directory.Move(_tempTransactionFolder, _finalTransactionFolder);
        }
    }
}
