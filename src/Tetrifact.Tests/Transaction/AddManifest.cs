using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Transaction
{
    [Collection("Tests")]
    public class AddManifest : FileSystemBase
    {
        /// <summary>
        /// Writing a manifest to a non-existent project throws useful exception
        /// </summary>
        [Fact]
        public void InvalidProject() 
        {
            Assert.Throws<ProjectCorruptException>(() =>
            {
                Core.Transaction transaction = new Core.Transaction(this.IndexReader, "random-invalid-project-name");
                transaction.AddPackage(new Package { Name = Guid.NewGuid().ToString() });
            });

        }
    }
}
