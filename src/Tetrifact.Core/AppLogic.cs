using System.IO;

namespace Tetrifact.Core
{
    public class AppLogic : IApplicationLogic
    {
        private readonly ITetriSettings _settings;

        public AppLogic(ITetriSettings settings) 
        {
            _settings = settings;
        }

        public void Start() 
        {
            FileHelper.EnsureDirectoryExists(_settings.ArchivePath);

            // force wipe the temp folder when app starts
            try {
                if (Directory.Exists(_settings.TempPath))
                    Directory.Delete(_settings.TempPath, true);
                Directory.CreateDirectory(_settings.TempPath);
            }
            catch (IOException) 
            {
                // if temp is folder is locked, ignore deleting it
            }

            FileHelper.EnsureDirectoryExists(_settings.ProjectsPath);
            FileHelper.EnsureDirectoryExists(_settings.TempPath);
            FileHelper.EnsureDirectoryExists(_settings.TempBinaries);
        }
    }
}
