using System.Collections.Generic;
using Xunit;
using Tetrifact.Core;
using Moq;
using System.Linq;

namespace Tetrifact.Tests.IndexReader
{
    public class FindExisting
    {
        private MoqHelper _moqHelper;

        private TestContext _testContext = new TestContext();
        
        public FindExisting()
        {
            _moqHelper = new MoqHelper(_testContext);
        }
        
        [Fact]
        public void HappyPath()
        {
        
            // mock some existing files
            IList<string> mockFilePaths = new List<string>() { "file1", "file2", "file3"};

            // mock up file system to match on the above path
            Mock<TestFileSystem> filesystem = _moqHelper.Mock<TestFileSystem>();
            filesystem
                .Setup(r => r.Directory.Exists(It.IsAny<string>()))
                .Returns((string pathToCheck)=>
                    {
                        foreach (var file in mockFilePaths) 
                            if (pathToCheck.Contains(file))
                                return true;

                        return false;
                    }
                );

            IIndexReadService indexReader = _moqHelper.CreateInstanceWithDependencies<IndexReadService>(new object[] { filesystem });

            PartialPackageLookupResult lookup = indexReader.FindExisting(new PartialPackageLookupArguments{Files = new List<ManifestItem>{
                new ManifestItem{Path = "file1", Hash = "" },
                new ManifestItem{Path = "file3", Hash = "" }
            }});
            
            Assert.Equal(2, lookup.Files.Count());
            Assert.Contains<string>("file1", lookup.Files.Select(r => r.Path));
            Assert.Contains<string>("file3", lookup.Files.Select(r => r.Path));
        }
    }
}
