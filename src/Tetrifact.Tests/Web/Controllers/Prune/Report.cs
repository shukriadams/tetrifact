using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Prune;

public class Report
{
    private TestContext _testContext = new TestContext();
    
    [Fact]
    public void Happy_path()
    {
        PruneController controller = _testContext.Instantiate<PruneController>();
        HttpHelper.EnsureContext(controller);
        
        string report = controller.Report();
        Assert.Contains("Total packages in system: 0", report);
    }
}