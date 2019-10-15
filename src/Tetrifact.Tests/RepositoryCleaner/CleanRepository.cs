using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.repositoryCleaner
{
    public class CleanRepository : FileSystemBase
    {
        private readonly IRepositoryCleaner _respositoryCleaner;

        private string CreateRepoContent()
        {
            string hash = "somehash";
            string path = "some/path/filename.file";
            string content = "file content";
            string rootPath = Path.Combine(base.Settings.RepositoryPath, path, hash);
            Directory.CreateDirectory(rootPath);
            string filePath = Path.Combine(rootPath, "bin");
            File.WriteAllText(filePath, content);

            return filePath;
        }

        public CleanRepository()
        {
            _respositoryCleaner = new RepositoryCleaner(this.IndexReader, this.Settings, new TestLogger<IRepositoryCleaner>());
        }

        [Fact]
        public void BasicClean()
        {
            // create a file and write to repository using path convention of path/to/file/bin. File is 
            // not linked to any package
            string contentPath = CreateRepoContent();

            // ensure file exists
            Assert.True(File.Exists(contentPath));

            // assert file is gone after cleaning repo
            _respositoryCleaner.Clean();
            Assert.False(File.Exists(contentPath));
        }

        /*
        [Fact]
        public void LinkLocked()
        {
            TestPackage package = base.CreatePackage();
            Core.LinkLock.Instance.Lock(package.Name);
            Settings.LinkLockWaitTime = 1; // millisecond
            int ticks = 0;


            Task.Run(() =>
            {
                _respositoryCleaner.Clean();
            });
            
            while(ticks < 10){
                ticks ++;    
                Thread.Sleep(10);
            }
            Core.LinkLock.Instance.Lock(package.Name);            
        }
        */
    }
}
