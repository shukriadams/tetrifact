using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Tetrifact.Core
{
    public class ProjectService : IProjectService
    {
        ITetriSettings _settings;
        ILogger<IProjectService> _logger;

        public ProjectService(ITetriSettings settings, ILogger<IProjectService> logger) 
        {
            _settings = settings;
            _logger = logger;
        }

        public ProjectCreateResult Create(string name)
        {
            try
            {
                string projectsRoot = Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(name));
                if (Directory.Exists(projectsRoot))
                    return new ProjectCreateResult { Success = false, PublicError = "Project already exists" };

                Directory.CreateDirectory(projectsRoot);

                Directory.CreateDirectory(Path.Combine(projectsRoot, Constants.ManifestsFragment));
                Directory.CreateDirectory(Path.Combine(projectsRoot, Constants.TransactionsFragment));
                Directory.CreateDirectory(Path.Combine(projectsRoot, Constants.ShardsFragment));

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
