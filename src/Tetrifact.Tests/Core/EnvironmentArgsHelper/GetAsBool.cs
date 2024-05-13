using System;
using Xunit;
using T = Tetrifact.Core;

namespace Tetrifact.Tests.EnvironmentArgsHelper
{
    public class GetAsBool : TestBase
    {
        [Fact]
        public void GetBoolTrueStringLower() 
        {
            string key = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(key, "true");

            bool value = T.EnvironmentArgsHelper.GetAsBool(key);
            Assert.True(value);
        }

        [Fact]
        public void GetBoolFalseStringLower()
        {
            string key = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(key, "false");

            bool value = T.EnvironmentArgsHelper.GetAsBool(key);
            Assert.False(value);
        }

        /// <summary>
        /// case of env var string value should be ignored
        /// </summary>
        [Fact]
        public void GetBoolStringRandomCse()
        {
            string key = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(key, "tRuE");

            bool value = T.EnvironmentArgsHelper.GetAsBool(key);
            Assert.True(value);
        }

        /// <summary>
        /// True can be set as as "1" sting
        /// </summary>
        [Fact]
        public void GetAsIntStringTrue() 
        {
            string key = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(key, "1");

            bool value = T.EnvironmentArgsHelper.GetAsBool(key);
            Assert.True(value);
        }

        /// <summary>
        /// False can be set as as "0" sting
        /// </summary>
        [Fact]
        public void GetAsIntStringFalse()
        {
            string key = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable(key, "0");

            bool value = T.EnvironmentArgsHelper.GetAsBool(key);
            Assert.False(value);
        }

        /// <summary>
        /// Trying to get an undefined env var should return false
        /// </summary>
        [Fact]
        public void GetNonExistingValue()
        {
            string key = Guid.NewGuid().ToString();

            bool value = T.EnvironmentArgsHelper.GetAsBool(key);
            Assert.False(value);
        }
    }
}
