﻿using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Rehydration
{
    public class Rehydration : FileSystemBase
    {
        /// <summary>
        /// Creates multiple packages with a single file, changing file contents per package. Retrieves the file from each version. Ensures that file is rehydrated from patches.
        /// </summary>
        [Fact]
        public void Basic() 
        {
            string contentBase = "some content";
            string content = "";
            int steps = 5;

            for (int i = 0; i < steps; i++) 
            {
                content = content + contentBase;
                Stream fileStream = StreamsHelper.StreamFromString(content);

                PackageCreate.Create(new PackageCreateArguments
                {
                    Id = $"my package{i}",
                    Project = "some-project",
                    Files = new List<IFormFile>() { (new FormFile(fileStream, 0, fileStream.Length, "Files", "folder/file")) }
                });
            }


            string expectedContent = "";
            for (int i = 0; i < steps; i++)
            {
                expectedContent = expectedContent + contentBase;
                GetFileResponse response = IndexReader.GetFile("some-project", Core.FileIdentifier.Cloak($"my package{i}", "folder/file" ));
                using (StreamReader reader = new StreamReader(response.Content))
                {
                    string retrievedContent = reader.ReadToEnd();
                    Assert.Equal(expectedContent, retrievedContent);
                }
            }

            Assert.True(File.Exists(this.IndexReader.GetItemPathOnDisk("some-project", "my package0", "folder/file/bin")));
            Assert.True(File.Exists(this.IndexReader.GetItemPathOnDisk("some-project", "my package1", "folder/file/patch")));
            Assert.True(File.Exists(this.IndexReader.GetItemPathOnDisk("some-project", "my package2", "folder/file/patch")));
            Assert.True(File.Exists(this.IndexReader.GetItemPathOnDisk("some-project", "my package3", "folder/file/patch")));
            Assert.True(File.Exists(this.IndexReader.GetItemPathOnDisk("some-project", "my package4", "folder/file/patch")));
        }
    }
}
