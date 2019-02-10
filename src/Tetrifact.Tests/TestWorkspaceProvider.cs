using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class TestWorkspaceProvider : IWorkspaceProvider
    {
        public static TestingWorkspace Instance;

        public static void Reset()
        {
            Instance = new TestingWorkspace();
        }

        static TestWorkspaceProvider()
        {
            Reset();
        }

        public IWorkspace Get()
        {
            return Instance;
        }
    }
}
