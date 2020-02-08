using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Tetrifact.Core
{
    public class ProjectService : IProjectService
    {
        ILogger<IProjectService> _logger;
        IPackageList _packageList;

        public ProjectService(IPackageList packageList, ILogger<IProjectService> logger) 
        {
            _logger = logger;
            _packageList = packageList;
        }

        public ProjectCreateResult Create(string name)
        {
            try
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
            catch (Exception ex) 
            {
                _logger.LogError("Unexpected error", ex);
                return new ProjectCreateResult { Success = false, PublicError = "Unexpected error - check logs." };
            }
        }
    }
}
