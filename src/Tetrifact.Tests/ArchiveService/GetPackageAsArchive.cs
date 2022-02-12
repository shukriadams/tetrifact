﻿using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageAsArchive : FileSystemBase
    {
        [Fact]
        public void GetBasic()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);
            using (Stream testContent = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
            {
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent);
                Assert.Single(items);
                Assert.Equal(testPackage.Content, items[testPackage.Path]);
            }
        }

        /// <summary>
        /// Ensures test coverage of code in GetPackageAsArchive() that references an existing archive
        /// file. To do this we hit the GetBasic() test, which generates an archive, then retrieve it again.
        /// The assert here is for completeness only - this test's passing requirement is measured in test
        /// coverage.
        /// 
        /// To confirm this test, uncomment it, run tests with coverage, generate report, and look for the branch
        /// warning in GetPackageAsArchive().
        /// </summary>
        [Fact]
        public void GetExistingArchive()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);
            using (Stream testContent1 = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
            {
                // get again
                using (Stream testContent2 = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
                {
                    Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent2);
                    Assert.Single(items);
                }
            }
        }

        /// <summary>
        /// Tests graceful handling of archive fetching for a non-existent package
        /// </summary>
        [Fact]
        public void GetNonExistent()
        {
            PackageNotFoundException ex = Assert.Throws<PackageNotFoundException>(() => {
                using (Stream zipStream = this.ArchiveService.GetPackageAsArchive("invalid id"))
                {
                    // do nothing, exception expected
                }
            });

            Assert.Equal("invalid id", ex.PackageId);
        }

        /// <summary>
        /// Confirms that a timeout exception is thrown if package creation takes too long.
        /// </summary>
        [Fact/* (Skip = "locking file doesn't work on linux, find cross-platform solution")*/]
        public void GetTimesOut()
        {
            // zero wait times to speed up test, this should trigger an instant timeout
            base.Settings.ArchiveWaitTimeout = 0;
            base.Settings.ArchiveAvailablePollInterval = 0;

            // we need a valid package first
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // mock a temp archive file and lock it to simulate an ongoing zip
            string tempArchivePath = this.ArchiveService.GetPackageArchiveTempPath(testPackage.Id);
            File.WriteAllText(tempArchivePath, string.Empty);

            using (FileStream lockStream = new FileStream(tempArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // lock the entire file
                lockStream.Lock(0, lockStream.Length);

                Assert.Throws<TimeoutException>(() => {
                    using (Stream zipStream = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
                    {
                        // do nothing, exception expected
                    }
                });
            }
        }

        /// <summary>
        /// Confirms that waiting-for-archive to generate goes into loop, which exits when the archive temp file is replaced with the completed archive.
        /// </summary>
        [Fact]
        public void GetAfterWaiting()
        {
            // we need a valid package first
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // create a fake archive temp file so GetPackageAsArchive() goes into wait state
            string tempArchivePath = this.ArchiveService.GetPackageArchiveTempPath(testPackage.Id);
            File.WriteAllText(tempArchivePath, string.Empty);

            // seeing as our fake temp archive bypasses archive creation, we should make a fake archive too, it should just be a file
            string archivePath = this.ArchiveService.GetPackageArchivePath(testPackage.Id);
            File.WriteAllText(archivePath, "some-fake-archive-content");

            // lock the temp archive so it doesn't get auto-clobbered
            using (Stream lockStream = new FileStream(tempArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // mock IThread.Sleep, in this method, we will release and delete our temp archive lock, so the archive fetch can proceed
                IThread mockThread = Mock.Of<IThread>();
                Mock.Get(mockThread)
                    .Setup(r => r.Sleep( It.IsAny<int>() ))
                    .Callback<int>(time =>{
                        lockStream.Close();
                        File.Delete(tempArchivePath);
                    });

                // make a custom reader with our mocked Thread
                IArchiveService archiveService = new Core.ArchiveService(this.IndexReader, mockThread, new FileSystem(), this.ArchiveLogger, this.Settings);

                using (Stream zipStream = archiveService.GetPackageAsArchive(testPackage.Id))
                {
                    // confirm we got our fake archive content back
                    Assert.Equal("some-fake-archive-content", StreamsHelper.StreamToString(zipStream));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void GetArchiveCompressionEnabled()
        {
            base.Settings.IsStorageCompressionEnabled = true;

            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);
            using (Stream testContent = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
            {
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent);
                Assert.Single(items);
                Assert.Equal(testPackage.Content, items[testPackage.Path]);
            }
        }

        /// <summary>
        /// Tests that trying to build an archive with a missing package file throws expected exception
        /// </summary>
        [Fact]
        public void GetArchive_Nocompress_FileMissing()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // delete known package file via disk
            File.Delete(Path.Join(this.Settings.RepositoryPath, testPackage.Path, testPackage.Hash, "bin"));

            Exception ex = Assert.Throws<Exception>(() => { this.ArchiveService.GetPackageAsArchive(testPackage.Id); });
            Assert.Contains("Failed to find expected package file", ex.Message);
        }

        /// <summary>
        /// Tests that trying to build an archive with a missing package file throws expected exception, compression of source files enabled. This
        /// is for coverage.
        /// </summary>
        [Fact]
        public void GetArchive_CompressEnabled_FileMissing()
        {
            base.Settings.IsStorageCompressionEnabled = true;

            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // delete known package file via disk
            File.Delete(Path.Join(this.Settings.RepositoryPath, testPackage.Path, testPackage.Hash, "bin"));

            Exception ex = Assert.Throws<Exception>(() => { this.ArchiveService.GetPackageAsArchive(testPackage.Id); });
            Assert.Contains("Failed to find expected package file", ex.Message);
        }

        /// <summary>
        /// Ensure that existing temp file is deleted if new archive is generated
        /// </summary>
        [Fact]
        public void GetArchive_Preexisting_tempFile()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // create abandoned tempfile for this package archive
            string tempArchivePath = this.ArchiveService.GetPackageArchiveTempPath(testPackage.Id);
            Directory.CreateDirectory(Path.GetDirectoryName(tempArchivePath));
            File.WriteAllText(tempArchivePath, string.Empty);

            // remove wait time, 
            Settings.ArchiveWaitTimeout = 0;

            // attempt to start a new archive generation, this should exit immediately
            using (Stream testContent = this.ArchiveService.GetPackageAsArchive(testPackage.Id))
            {
                Assert.True(ArchiveLogger.ContainsFragment($"Deleted abandoned temp archive for {testPackage.Id}"));
            }
        }

        /// <summary>
        /// Ensure graceful handling of locked temp file from previous archive generating attempt
        /// </summary>
        [Fact/* (Skip = "fails on travis")*/]
        public void GetArchive_Preexisting_locked_tempFile()
        {
            TestPackage testPackage = PackageHelper.CreatePackage(this.Settings);

            // create and lock tempfile for this package archive
            string tempArchivePath = this.ArchiveService.GetPackageArchiveTempPath(testPackage.Id);
            Directory.CreateDirectory(Path.GetDirectoryName(tempArchivePath));
            File.WriteAllText(tempArchivePath, string.Empty);
            
            // remove wait time, 
            Settings.ArchiveWaitTimeout = 0;

            using (FileStream lockStream = new FileStream(tempArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // lock the entire file
                lockStream.Lock(0, lockStream.Length);

                // attempt to start a new archive generation, this should exit immediately
                TimeoutException ex = Assert.Throws<TimeoutException>(() => { this.ArchiveService.GetPackageAsArchive(testPackage.Id); });
                Assert.NotNull(ex);
                Assert.True(ArchiveLogger.ContainsFragment("skipped, existing process detected"));
            }
        }
    }
}
