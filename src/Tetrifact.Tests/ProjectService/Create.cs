using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests
{
    [Collection("Tests")]
    public class ProjectService : FileSystemBase
    {
        /// <summary>
        /// ProjectService.Create creates a project folder
        /// </summary>
        [Fact]
        public void Basic()
        {
            string project = Guid.NewGuid().ToString();
            this.ProjectService.Create(project);

            IEnumerable<string> projects = Directory.GetDirectories(Settings.ProjectsPath).Select(r => Path.GetFileName(r));
            Assert.Contains(Obfuscator.Cloak(project), projects);
        }

        /// <summary>
        /// ProjectService.Create creates a folder which contains required child directories
        /// </summary>
        [Fact]
        public void DirectoryStructure()
        {
            string project = Guid.NewGuid().ToString();
            this.ProjectService.Create(project);

            string projectPathOnDisk = Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project));
            IEnumerable<string> projects = Directory.GetDirectories(projectPathOnDisk);
            Assert.Equal(3, projects.Count());
            Assert.True(Directory.Exists(Path.Combine(projectPathOnDisk, Constants.ManifestsFragment)));
            Assert.True(Directory.Exists(Path.Combine(projectPathOnDisk, Constants.ShardsFragment)));
            Assert.True(Directory.Exists(Path.Combine(projectPathOnDisk, Constants.TransactionsFragment)));
        }

        /// <summary>
        /// ProjectService.Create creates a project folder from characters which are not allowed on filesystem
        /// </summary>
        [Fact]
        public void IllegalCharacterSupport()
        {
            string project = ": // \\ *";
            this.ProjectService.Create(project);

            IEnumerable<string> projects = Directory.GetDirectories(Settings.ProjectsPath).Select(r => Path.GetFileName(r));
            Assert.Contains(Obfuscator.Cloak(project), projects);
        }
    }
}
