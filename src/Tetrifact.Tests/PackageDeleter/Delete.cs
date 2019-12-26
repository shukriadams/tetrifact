using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tetrifact.Core;
using Tetrifact.Dev;
using Xunit;

namespace Tetrifact.Tests.PackageDeleter
{
    public class Delete : FileSystemBase
    {
        #region FIELDS

        private IPackageDeleter PackageDeleter;

        #endregion

        #region CTORS

        public Delete() 
        {
            this.PackageDeleter = new Core.PackageDeleter(this.IndexReader, this.Settings, new MemoryLogger<IPackageDeleter>(), new MemoryLogger<IPackageCreate>());
        }

        #endregion

        #region TESTS

        /// <summary>
        /// Preceeding package becomes head if deleting last package in project
        /// </summary>
        [Fact]
        public void HeadToPreceeding() 
        {
            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
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
            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
            {
                Id = "second",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
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
            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
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
            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "path/to/file")
            });

            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
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
            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
            {
                Id = "first",
                Project = "some-project",
                Files = FormFileHelper.Single("1", "path/to/file")
            });

            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
            {
                Id = "second",
                Project = "some-project",
                Files = FormFileHelper.Single("12", "path/to/file")
            });

            this.PackageCreate.CreateWithValidation(new PackageCreateArguments
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

        #endregion
    }
}
