using Tetrifact.Web;
using Xunit;
using Ninject;
using Tetrifact.Core;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Tetrifact.Tests.Controllers
{
    [Collection("Tests")]
    public class Projects : TestBase
    {
        #region FIELDS

        private readonly ProjectsController _controller;

        #endregion

        #region CTORS
        
        public Projects()
        {
            _controller = this.Kernel.Get<ProjectsController>();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// AddProject returns 200.
        /// </summary>
        [Fact]
        public void AddProject()
        {
            ProjectCreateArguments args = new ProjectCreateArguments {Project = Guid.NewGuid().ToString() };
            OkObjectResult result =  _controller.AddProject(args) as OkObjectResult;
            Assert.NotNull(result);
        }

        #endregion
    }
}
