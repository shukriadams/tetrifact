using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Tetrifact.Tests
{
    public class TestHostApplicationLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted
        {
            get
            {
                return CancellationToken.None;
            }
        }


        public CancellationToken ApplicationStopped
        { 
            get 
            { 
                return CancellationToken.None; 
            }
        }

        public CancellationToken ApplicationStopping 
        { 
            get 
            { 
                return CancellationToken.None; 
            } 
        }

        public void StopApplication()
        {
            // do nothing
        }
    }
}
