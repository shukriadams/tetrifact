using System.IO;

namespace Tetrifact.Core
{
    public class AppLogic : IApplicationLogic
    {
        public void Start() 
        {
            FileHelper.EnsureDirectoryExists(Settings.ArchivePath);

            // force wipe the temp folder when app starts
            try {
                if (Directory.Exists(Settings.TempPath))
                    Directory.Delete(Settings.TempPath, true);
                Directory.CreateDirectory(Settings.TempPath);
            }
            catch (IOException) 
            {
                // if temp is folder is locked, ignore deleting it
            }

            FileHelper.EnsureDirectoryExists(Settings.ProjectsPath);
            FileHelper.EnsureDirectoryExists(Settings.TempPath);
            FileHelper.EnsureDirectoryExists(Settings.TempBinaries);
        }
    }
}
