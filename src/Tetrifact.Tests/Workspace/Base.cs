using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Base : FileSystemBase
    {
        protected IWorkspace Workspace;

        public Base()
        {
            Workspace = new Core.Workspace(Settings, base.WorkspaceLogger);
            Workspace.Initialize("some-project");
        }
    }
}
