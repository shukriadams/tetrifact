using Microsoft.AspNetCore.Mvc;
using Tetrifact.Web;
using Xunit;

namespace Tetrifact.Tests.Web.Controllers.Errors;

public class Configuration
{
    private TestContext _testContext = new TestContext();
    
    [Fact]
    public void Happy_path()
    {
        ErrorsController controller = _testContext.Instantiate<ErrorsController>();
        HttpHelper.EnsureContext(controller);
        
        IActionResult result = controller.Configuration() as IActionResult;
        Assert.NotNull(result);
    }
}