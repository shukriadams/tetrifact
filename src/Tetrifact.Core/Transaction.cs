using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    public class Transaction
    {
        private readonly string _finalTransactionFolder;
        private readonly string _tempTransactionFolder;
        private readonly IIndexReader _indexReader;
        private readonly ISettings _settings;

        public Transaction(ISettings settings, IIndexReader indexReader, string project) 
        {
            _indexReader = indexReader;
            _settings = settings;

            long ticks = DateTime.UtcNow.Ticks;
            _tempTransactionFolder = Path.Combine(settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment, $"~{ticks}");
            _finalTransactionFolder = Path.Combine(settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment, $"{ticks}");
            Directory.CreateDirectory(_tempTransactionFolder);

            // find current transaction and copy all files over 
            DirectoryInfo activeTransaction = _indexReader.GetActiveTransactionInfo(project);
            if (activeTransaction != null) 
                foreach (FileInfo file in activeTransaction.GetFiles())
                    file.CopyTo(Path.Combine(_tempTransactionFolder, file.Name));
        }

        /// <summary>
        /// Sets head. If package is null or empty, removes head.
        /// </summary>
        /// <param name="package"></param>
        public void SetHead(string package) 
        {
            string path = Path.Combine(_tempTransactionFolder, "head");
            if (string.IsNullOrEmpty(package))
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            else 
            {
                File.WriteAllText(path, package);
            }
        }

        public void AddManifest(string project, Manifest manifest) 
        {
            if (string.IsNullOrEmpty(manifest.Id))
                throw new Exception("Manifest id not set");

            string fileName = $"{Guid.NewGuid()}_{manifest.Id}";
            File.WriteAllText(Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment, fileName), JsonConvert.SerializeObject(manifest));

            // write pointer, this overwrites existing pointer
            File.WriteAllText(Path.Combine(_tempTransactionFolder, $"{Obfuscator.Cloak(manifest.Id)}_manifest"), fileName);
        }

        public void AddShardPointer(string package, string targetName) 
        {
            File.WriteAllText(Path.Combine(_tempTransactionFolder, $"{Obfuscator.Cloak(package)}_shard"), targetName);
        }

        public void AddDependecy(string parent, string child, bool isExplicity)
        {
            string explicitSwitch = isExplicity ? "_!" : string.Empty;
            File.WriteAllText(Path.Combine(_tempTransactionFolder, $"dep_{Obfuscator.Cloak(parent)}_{Obfuscator.Cloak(child)}{explicitSwitch}"), string.Empty);
        }

        public void Remove(string package) 
        {
            this.RemoveManifestPointer(package);
            this.RemoveShardPointer(package);

            IEnumerable<string> dependencyLinks = Directory.GetFiles(_tempTransactionFolder, $"dep_*_{Obfuscator.Cloak(package)}*");
            foreach (string dependencyLink in dependencyLinks)
                File.Delete(dependencyLink);

            dependencyLinks = Directory.GetFiles(_tempTransactionFolder, $"dep_{Obfuscator.Cloak(package)}_*");
            foreach (string dependencyLink in dependencyLinks)
                File.Delete(dependencyLink);
        }

        public void RemoveManifestPointer(string package)
        {
            string path = Path.Combine(_tempTransactionFolder, $"{Obfuscator.Cloak(package)}_manifest");
            if (File.Exists(path))
                File.Delete(path);
        }

        public void RemoveShardPointer(string package)
        {
            string path = Path.Combine(_tempTransactionFolder, $"{Obfuscator.Cloak(package)}_shard");
            if (File.Exists(path))
                File.Delete(path);
        }

        public void RemoveDependency(string parent, string child)
        {
            string path = Path.Combine(_tempTransactionFolder, $"dep_{Obfuscator.Cloak(parent)}_{Obfuscator.Cloak(child)}");
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
