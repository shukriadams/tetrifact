using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Tetrifact.Core
{
    public class Cleaner : ICleaner
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<ICleaner> _logger;

        private readonly IIndexReader _indexReader;

        #endregion

        #region CTORS

        public Cleaner(IIndexReader indexReader, ISettings settings, ILogger<ICleaner> logger)
        {
            _settings = settings;
            _logger = logger;
            _indexReader = indexReader;
        }

        #endregion

        #region METHODS

        public void Clean(string project)
        {
            TransactionHelper transactionHelper = new TransactionHelper(_indexReader, _settings);
            ProjectCurrentState projectView = transactionHelper.GetProjectView(project);

            List<string> filesToDelete = new List<string>();
            List<string> directoriesToDelete = new List<string>();

            // find all transaction not in history view
            string[] allTransactions = Directory.GetDirectories(PathHelper.ResolveTransactionRoot(_settings, project));
            TimeSpan transactionTimeout = new TimeSpan(24, 0, 0);

            foreach (string existingTransaction in allTransactions)
            {
                // handle uncommitted transactions
                if (Path.GetFileName(existingTransaction).StartsWith("~")) 
                {
                    // if transaction has timed out, add to delete list, else ignore it
                    DirectoryInfo dirInfo = new DirectoryInfo(existingTransaction);
                    if (DateTime.UtcNow - dirInfo.CreationTimeUtc > transactionTimeout)
                        directoriesToDelete.Add(existingTransaction);

                    continue;
                }
                
                // if transactiotn has already been flagged for delete, the previous clean run attempted 
                // and failed to delete it. Add to delete list to try again.
                if (Path.GetFileName(existingTransaction).StartsWith(PathHelper.DeleteFlag))
                {
                    directoriesToDelete.Add(existingTransaction);
                    continue;
                }

                if (!projectView.Transactions.Contains(Path.GetFileName(existingTransaction)))
                {
                    try
                    {
                        string targetPath = PathHelper.GetDeletingPath(existingTransaction);
                        Directory.Move(existingTransaction, targetPath);
                        directoriesToDelete.Add(targetPath);
                    }
                    catch (IOException)
                    {
                        // ignore, content is in use, we'll delete on next clean
                    }
                }
            }


            // find all shards not in history view
            string[] allShards = Directory.GetDirectories(PathHelper.ResolveShardRoot(_settings, project));
            foreach (string existingShard in allShards)
            {
                if (Path.GetFileName(existingShard).StartsWith(PathHelper.DeleteFlag))
                {
                    directoriesToDelete.Add(existingShard);
                    continue;
                }

                if (!projectView.Shards.Contains(Path.GetFileName(existingShard)))
                {
                    try
                    {
                        string targetPath = PathHelper.GetDeletingPath(existingShard);
                        Directory.Move(existingShard, targetPath);
                        directoriesToDelete.Add(targetPath);
                    }
                    catch (IOException)
                    {
                        // ignore, content is in use, we'll delete on next clean
                    }
                }
            }


            // find all manifests not in history view
            string[] allManifests = Directory.GetFiles(PathHelper.ResolveManifestsRoot(_settings, project));
            foreach (string existingManifest in allManifests)
            {
                if (Path.GetFileName(existingManifest).StartsWith(PathHelper.DeleteFlag))
                {
                    filesToDelete.Add(existingManifest);
                    continue;
                }

                if (!projectView.Manifests.Contains(Path.GetFileName(existingManifest)))
                {
                    try
                    {
                        string targetPath = PathHelper.GetDeletingPath(existingManifest);
                        File.Move(existingManifest, targetPath);
                        filesToDelete.Add(targetPath);
                    }
                    catch (IOException)
                    {
                        // ignore, content is in use, we'll delete on next clean
                    }
                }
            }


            foreach (string item in directoriesToDelete)
            {
                try
                {
                    Directory.Delete(item, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error trying to delete {item}", ex);
                }
            }


            foreach (string item in filesToDelete)
            {
                try
                {
                    File.Delete(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error trying to delete {item}", ex);
                }
            }
        }

        #endregion
    }
}
