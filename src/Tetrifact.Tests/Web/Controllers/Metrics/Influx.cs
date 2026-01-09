using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Metrics;

public class Influx
{
    private TestContext _testContext = new TestContext();

    /// <summary>
    /// By default, influx metrics should not be available on a clean system. It needs to be explicitly generated firrst.
    /// </summary>
    [Fact]
    public void Influx_Not_Generated_By_Default()
    {
        MetricsController controller = _testContext.Instantiate<MetricsController>();
        HttpHelper.EnsureContext(controller);
        
        dynamic result = JsonHelper.ToDynamic(controller.Influx());
        Assert.Contains("Influx metrics file not found. File might not have been generated yet.", result.Result.Value.error.description.ToString());
    }
}