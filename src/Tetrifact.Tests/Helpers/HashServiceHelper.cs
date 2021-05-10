using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class HashServiceHelper
    {
        public static IHashService Instance() 
        {
            return new HashService();
        }
    }
}
