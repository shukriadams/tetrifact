using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tetrifact.Core;
using Tetrifact.Dev;
using Xunit;

namespace Tetrifact.Tests.PackageDeleter
{
    [Collection("Tests")]
    public class Delete : FileSystemBase
    {
        #region TESTS

        /// <summary>
        /// Preceeding package becomes head if deleting last package in project
        /// </summary>
        [Fact]
        public void HeadToPreceeding() 
        {
            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "second",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageDeleter.Delete("some-project", "second");

            // create two packages. delete the second one. the first should be head again.
            Assert.Equal("first", this.IndexReader.GetHead("some-project"));
        }

        /// <summary>
        /// If deleted package has dependents, head remains unchanged
        /// </summary>
        [Fact]
        public void HeadUnchanged()
        {
            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "second",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "third",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageDeleter.Delete("some-project", "second");

            // create three packages. delete the middle one. the last should still be head
            Assert.Equal("third", this.IndexReader.GetHead("some-project"));
        }

        /// <summary>
        /// Head cleared when deleting last package in project.
        /// </summary>
        [Fact]
        public void HeadClearAscending()
        {
            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "second",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            // create two packages. delete in ascending order. head should be null
            this.PackageDeleter.Delete("some-project", "first");
            this.PackageDeleter.Delete("some-project", "second");
            Assert.Null(this.IndexReader.GetHead("some-project"));
        }

        /// <summary>
        /// Head cleared when deleting last package in project.
        /// </summary>
        [Fact]
        public void HeadClearDescending()
        {
            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "second",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            // create two packages. delete in descending order. head should be null
            this.PackageDeleter.Delete("some-project", "second");
            this.PackageDeleter.Delete("some-project", "first");
            Assert.Null(this.IndexReader.GetHead("some-project"));
        }

        /// <summary>
        /// Deleting packages patched against will not affect the content stack.
        /// </summary>
        [Fact]
        public void DeleteConsistencyCheck() 
        {
            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("1", "path/to/file")
            });

            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "second",
                Project = "some-project",
                Files = FormFileHelper.Single("12", "path/to/file")
            });

            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "third",
                Project = "some-project",
                Files = FormFileHelper.Single("23", "path/to/file")
            });

            this.PackageDeleter.Delete("some-project", "first");
            this.PackageDeleter.Delete("some-project", "second");

            using (Stream testContent = this.IndexReader.GetPackageAsArchive("some-project", "third"))
            {
                Dictionary<string, byte[]> items = StreamsHelper.ArchiveStreamToCollection(testContent);
                Assert.Single(items);
                Assert.Equal(Encoding.ASCII.GetBytes("23"), items[items.Keys.First()]);
            }
        }

        /// <summary>
        /// Dependats of a given package are still viable after that package is deleted.
        /// </summary>
        [Fact]
        public void DeleteLinked() 
        {
            // create two packages, the second with linked content
            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.Create(new PackageCreateArguments
            {
                Id = "second",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            // delete first to force second to take on the content of first
            this.PackageDeleter.Delete("some-project", "first");

            // confirm first package has been deleted, this has failed before
            Assert.False(this.IndexReader.PackageNameInUse("some-project", "first"));

            // confirm content is avaialble in second
            GetFileResponse fileResponse = this.IndexReader.GetFile("some-project", FileIdentifier.Cloak("second", "path/to/file"));
            Assert.Equal("some content", StreamsHelper.StreamToString(fileResponse.Content));
            fileResponse.Content.Dispose();

            // confirm second is no longer linked but now fully owns content
            Package manifest = this.IndexReader.GetPackage("some-project", "second");
            Assert.Single(manifest.Files);
            Assert.Equal(ManifestItemTypes.Bin, manifest.Files[0].Chunks[0].Type);
        }

        #endregion
    }
}
