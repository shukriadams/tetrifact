using System.IO;

namespace Tetrifact.Core
{
    public class ProjectService : IProjectService
    {
        IPackageList _packageList;

        public ProjectService(IPackageList packageList) 
        {
            _packageList = packageList;
        }

        public ProjectCreateResult CreateProject(string name)
        {
            string projectsRoot = Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(name));
            if (Directory.Exists(projectsRoot))
                return new ProjectCreateResult { Success = false, PublicError = "Project already exists" };

            Directory.CreateDirectory(projectsRoot);

            Directory.CreateDirectory(Path.Combine(projectsRoot, Constants.ManifestsFragment));
            Directory.CreateDirectory(Path.Combine(projectsRoot, Constants.TransactionsFragment));
            Directory.CreateDirectory(Path.Combine(projectsRoot, Constants.ShardsFragment));

            _packageList.Clear();
            return new ProjectCreateResult { Success = true };
        }

        public void DeleteProject(string project) 
        {
            string projectsFolder = Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project));
            if (!Directory.Exists(projectsFolder))
                return;

            string deleteName = Path.Combine(Path.GetDirectoryName(projectsFolder), $"!{projectsFolder}");
            if (!Directory.Exists(deleteName))
                Directory.Move(projectsFolder, deleteName);

            Directory.Delete(deleteName, true);
        }
    }
}
