using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Tetrifact.Core
{
    public class ProjectService : IProjectService
    {
        ITetriSettings _settings;
        ILogger<ProjectService> _logger;

        public ProjectService(ITetriSettings settings, ILogger<ProjectService> logger) 
        {
            _settings = settings;
            _logger = logger;
        }

        public ProjectCreateResult Create(string name)
        {
            try
            {
                string projectsRoot = Path.Combine(_settings.ProjectsPath, name);
                if (Directory.Exists(projectsRoot))
                    return new ProjectCreateResult { Success = false, PublicError = "Project already exists" };

                Directory.CreateDirectory(projectsRoot);

                string packagesPath = Path.Combine(projectsRoot, Constants.PackagesFragment);
                Directory.CreateDirectory(packagesPath);

                string repositoryPath = Path.Combine(projectsRoot, Constants.RepositoryFragment);
                Directory.CreateDirectory(repositoryPath);

                string tagsPath = Path.Combine(projectsRoot, Constants.TagsFragment);
                Directory.CreateDirectory(tagsPath);

                string headPath = Path.Combine(projectsRoot, Constants.HeadFragment);
                Directory.CreateDirectory(headPath);

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
