using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Transaction
    {
        #region FIELDS

        private readonly string _finalTransactionFolder;
        private readonly string _tempTransactionFolder;
        private readonly IIndexReader _indexReader;
        private string _project;

        #endregion

        #region CTORS

        public Transaction(IIndexReader indexReader, string project) 
        {
            _indexReader = indexReader;
            _project = project;

            long ticks = DateTime.UtcNow.Ticks;
            _tempTransactionFolder = Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment, $"~{ticks}");
            _finalTransactionFolder = Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment, $"{ticks}");
            Directory.CreateDirectory(_tempTransactionFolder);

            // find current transaction and copy all files over 
            DirectoryInfo activeTransaction = _indexReader.GetActiveTransactionInfo(project);
            if (activeTransaction != null) 
                foreach (FileInfo file in activeTransaction.GetFiles())
                    file.CopyTo(Path.Combine(_tempTransactionFolder, file.Name));
        }

        #endregion

        #region METHODS

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void AddPackage(Package package) 
        {
            if (string.IsNullOrEmpty(package.Name))
                throw new Exception("Manifest id not set");

            string fileName = $"{Guid.NewGuid()}_{package.Name}";
            try
            {
                File.WriteAllText(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(_project), Constants.ManifestsFragment, fileName), JsonConvert.SerializeObject(package));
            }
            catch (DirectoryNotFoundException ex) 
            {
                // this will rarely be reached - if so, try to derive a more helpful exception from missing directory exception

                // test projectPaths
                if (!Directory.Exists(Settings.ProjectsPath))
                    throw new SystemCorruptException("Projects folder not found");

                // test project folder
                if (!Directory.Exists(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(_project))))
                    throw new ProjectNotFoundException(_project);

                // test manifests fragment
                if (!Directory.Exists(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(_project), Constants.ManifestsFragment)))
                    throw new ProjectCorruptException(_project, "Manifests folder not found");

                throw ex;
            }

            // write pointer, this overwrites existing pointer
            File.WriteAllText(Path.Combine(_tempTransactionFolder, $"{Obfuscator.Cloak(package.Name)}_manifest"), fileName);
        }

        /// <summary>
        /// Publishes the content directory to shard folder. 
        /// </summary>
        /// <param name="package"></param>
        /// <param name="contentDirectory"></param>
        public void AddShard(string package, string contentDirectory) 
        {
            string packageNoCollideName = $"{Guid.NewGuid()}__{Obfuscator.Cloak(package)}";
            string shardRoot = PathHelper.ResolveShardRoot(_project);
            string finalRoot = Path.Combine(shardRoot, packageNoCollideName);
            FileHelper.MoveDirectoryContents(contentDirectory, finalRoot);

            // write pointer
            File.WriteAllText(Path.Combine(_tempTransactionFolder, $"{Obfuscator.Cloak(package)}_shard"), packageNoCollideName);
        }

        /// <summary>
        /// Writes an index flag linking a package to its parent. This is used to rapidly lookup children when deleting a package.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public void AddChildReference(string parent, string child)
        {
            File.WriteAllText(Path.Combine(_tempTransactionFolder, $"dep_{Obfuscator.Cloak(parent)}_{Obfuscator.Cloak(child)}_"), string.Empty);
        }

        /// <summary>
        /// Removes a package and all related files from the transaction.
        /// </summary>
        /// <param name="package"></param>
        public void Remove(string package) 
        {
            // remove manifest pointer
            string path = Path.Combine(_tempTransactionFolder, $"{Obfuscator.Cloak(package)}_manifest");
            if (File.Exists(path))
                File.Delete(path);

            // remove shard pointer
            path = Path.Combine(_tempTransactionFolder, $"{Obfuscator.Cloak(package)}_shard");
            if (File.Exists(path))
                File.Delete(path);

            IEnumerable<string> dependencyLinks = Directory.GetFiles(_tempTransactionFolder, $"dep_*_{Obfuscator.Cloak(package)}*");
            foreach (string dependencyLink in dependencyLinks)
                File.Delete(dependencyLink);

            dependencyLinks = Directory.GetFiles(_tempTransactionFolder, $"dep_{Obfuscator.Cloak(package)}_*");
            foreach (string dependencyLink in dependencyLinks)
                File.Delete(dependencyLink);
        }

        /// <summary>
        /// Renames the transaction to its final location, exposing it and "going live". This is a single step that either passes or fails, assuming the
        /// filesystem rename is atomic.
        /// </summary>
        public void Commit() 
        {
            // flip transaction live
            Directory.Move(_tempTransactionFolder, _finalTransactionFolder);
        }

        #endregion
    }
}
