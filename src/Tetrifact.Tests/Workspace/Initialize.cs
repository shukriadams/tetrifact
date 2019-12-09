using System.IO;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Initialize : Base
    {
        [Fact]
        public void Basic()
        {
            string project = "some-project";
            base.Workspace.Initialize(project);
            Assert.True(Directory.Exists(PathHelper.GetExpectedManifestsPath(base.Settings, project)));
            Assert.True(Directory.Exists(PathHelper.GetExpectedProjectPath(base.Settings, project)));
            Assert.True(Directory.Exists(PathHelper.GetExpectedRepositoryPath(base.Settings, project)));
            Assert.True(Directory.Exists(PathHelper.GetExpectedTagsPath(base.Settings, project)));
        }
    }
}
