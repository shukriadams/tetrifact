using Xunit;

namespace Tetrifact.Tests.Percent
{
    public class Calc
    {

        [Fact]
        public void LongTest()
        {
            long num = 100000000001;
            long den = 200000000000;
            int percent = Tetrifact.Core.Percent.Calc(num, den);
            Assert.Equal(50, percent);
        }
    }
}


