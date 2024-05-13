using System;
using Xunit;
using T = Tetrifact.Core;

namespace Tetrifact.Tests.EnvironmentArgsHelper
{
    public class GetAsInt : TestBase
    {
        [Fact]
        public void GetExisting() 
        {
            string key = Guid.NewGuid().ToString();
            int setValue = new Random().Next(0, 10000);
            Environment.SetEnvironmentVariable(key, setValue.ToString());

            int getValue = T.EnvironmentArgsHelper.GetAsInt(key, 0);
            Assert.Equal(setValue, getValue);
        }

        /// <summary>
        /// returns fallback value if undefined
        /// </summary>
        [Fact]
        public void GetUndefined()
        {
            string key = Guid.NewGuid().ToString();

            int getValue = T.EnvironmentArgsHelper.GetAsInt(key, 123);
            Assert.Equal(123, getValue);
        }
    }
}
