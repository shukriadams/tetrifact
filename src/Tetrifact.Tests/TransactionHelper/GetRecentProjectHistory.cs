﻿using Xunit;
using Tetrifact.Core;
using System.IO;
using System;
using System.Linq;

namespace Tetrifact.Tests.TransactionHelper
{
    public class GetRecentProjectHistory : FileSystemBase
    {
        [Fact]
        public void Get() 
        {
            // create first transation with 1 manifest in it
            Core.Transaction transaction = new Core.Transaction(this.Settings, this.IndexReader, "some-project");
            transaction.AddManifest(new Manifest { Id = Guid.NewGuid().ToString() });
            transaction.Commit();

            // create second transaction with 2 manifests and 1 shard
            transaction = new Core.Transaction(this.Settings, this.IndexReader, "some-project");
            transaction.AddManifest(new Manifest { Id = Guid.NewGuid().ToString() });
            transaction.AddManifest(new Manifest { Id = Guid.NewGuid().ToString() });

            string contentPath = Path.Combine(Settings.TempPath, Guid.NewGuid().ToString());
            Core.FileHelper.EnsureDirectoryExists(contentPath);
            transaction.AddShard("a-package", contentPath);

            transaction.Commit();

            // force depth to single, ie, last transaction
            Settings.TransactionHistoryDepth = 1;

            Core.TransactionHelper transactionHelper = new Core.TransactionHelper(this.IndexReader, this.Settings);
            ProjectRecentHistory history = transactionHelper.GetRecentProjectHistory("some-project");
            Assert.Single(history.Transactions);
            // must contain combined manifests of transaction 1 + 2
            Assert.Equal(3, history.Manifests.Count());
            Assert.Single(history.Shards);
        }
    }
}