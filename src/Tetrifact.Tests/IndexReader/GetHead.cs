using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetHead : PackageCreatorBase
    {

        /// <summary>
        /// Confirms that head is updated correctly over a series of package uploads
        /// </summary>
        [Fact]
        public void Sequence()
        {
            this.InitProject();

            for (int i = 0; i < 10; i++) 
            {
                Stream fileStream = StreamsHelper.StreamFromString($"content-{i}");

                // create package
                Assert.True(PackageCreate.CreatePackage(new PackageCreateArguments
                {
                    Id = $"my package{i}",
                    Project = "some-project",
                    Files = new List<IFormFile>() { new FormFile(fileStream, 0, fileStream.Length, "Files", $"folder/file") }
                }).Success);

                // confirm head is now at this package
                Assert.Equal($"my package{i}", IndexReader.GetHead("some-project"));
            }

        }
    }
}
