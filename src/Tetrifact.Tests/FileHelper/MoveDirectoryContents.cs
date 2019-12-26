using System.IO;
using Xunit;

namespace Tetrifact.Tests.FileHelper
{
    public class MoveDirectoryContents : FileSystemBase
    {
        /// <summary>
        /// Moves directory contents
        /// </summary>
        [Fact]
        public void Move() 
        {
            string source = Path.Combine(Settings.TempPath, "1");
            string target = Path.Combine(Settings.TempPath, "2");
            string file = Path.Combine(source, "file.txt");

            Core.FileHelper.EnsureDirectoryExists(source);
            Core.FileHelper.EnsureDirectoryExists(target);
            File.WriteAllText(file, string.Empty);
            
            Core.FileHelper.MoveDirectoryContents(source, target);

            // file exists at new location
            Assert.True(File.Exists(Path.Combine(target, "file.txt")));
            // file no longer exists at old location
            Assert.False(File.Exists(file));
        }
    }
}
