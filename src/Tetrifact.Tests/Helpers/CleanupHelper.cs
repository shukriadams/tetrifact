using System.IO;
using System.Linq;

namespace Tetrifact.Tests
{
    public class CleanupHelper
    {
        public static void ClearDirectory(string directoryPath)
        { 
            Directory.GetFiles(directoryPath)
                .ToList()
                .ForEach(file => File.Delete(file));

            Directory.GetDirectories(directoryPath)
                .ToList()
                .ForEach(directory => Directory.Delete(directory, true));
        }
    }
}
