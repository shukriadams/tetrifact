using System;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Settings
{
    public class Constructor 
    {
        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Compression_NoCompression()
        {
            Environment.SetEnvironmentVariable("DOWNLOAD_ARCHIVE_COMPRESSION", "0");
            ISettings settings = NinjectHelper.Get<ISettings>();
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Compression_MaxCompression()
        {
            Environment.SetEnvironmentVariable("DOWNLOAD_ARCHIVE_COMPRESSION", "1");
            ISettings settings = NinjectHelper.Get<ISettings>();
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Access_tokens()
        {
            Environment.SetEnvironmentVariable("ACCESS_TOKENS", "123");
            ISettings settings = NinjectHelper.Get<ISettings>();

            // revert
            Environment.SetEnvironmentVariable("ACCESS_TOKENS", "");
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Error_handle_invalid_integer_arg()
        {
            Environment.SetEnvironmentVariable("MAX_ARCHIVES", "zzz");
            ISettings settings = NinjectHelper.Get<ISettings>();

            // shoudl revert to default
            Assert.Equal(10, settings.MaxArchives);
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Valid_integer_arg()
        {
            Environment.SetEnvironmentVariable("MAX_ARCHIVES", "12");
            ISettings settings = NinjectHelper.Get<ISettings>();

            // shoudl revert to default
            Assert.Equal(12, settings.MaxArchives);

            Environment.SetEnvironmentVariable("MAX_ARCHIVES", "10");
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Error_handle_invalid_long_arg()
        {
            Environment.SetEnvironmentVariable("SPACE_SAFETY_THRESHOLD", "zzz");
            ISettings settings = NinjectHelper.Get<ISettings>();

            // shoudl revert to default
            Assert.Equal(0, settings.SpaceSafetyThreshold);
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Valid_long_arg()
        {
            Environment.SetEnvironmentVariable("SPACE_SAFETY_THRESHOLD", "10");
            ISettings settings = NinjectHelper.Get<ISettings>();

            Assert.Equal(10, settings.SpaceSafetyThreshold);
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Error_handle_invalid_bool_arg()
        {
            Environment.SetEnvironmentVariable("ALLOW_PACKAGE_CREATE", "zzz");
            ISettings settings = NinjectHelper.Get<ISettings>();

            // shoudl revert to default
            Assert.True(settings.AllowPackageCreate);

            // revert
            Environment.SetEnvironmentVariable("ALLOW_PACKAGE_CREATE", "true");
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Error_handle_enum_parse()
        {
            Environment.SetEnvironmentVariable("AUTH_LEVEL", "zzz");
            ISettings settings = NinjectHelper.Get<ISettings>();

            // shoudl revert to default
            Assert.Equal(AuthorizationLevel.None, settings.AuthorizationLevel);
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Handle_enum_parse()
        {
            Environment.SetEnvironmentVariable("AUTH_LEVEL", AuthorizationLevel.Read.ToString());
            ISettings settings = NinjectHelper.Get<ISettings>();

            // shoudl revert to default
            Assert.Equal(AuthorizationLevel.Read, settings.AuthorizationLevel);

            // force revert
            Environment.SetEnvironmentVariable("AUTH_LEVEL", AuthorizationLevel.Write.ToString());
        }

        /// <summary>
        /// Coverage 
        /// </summary>
        [Fact]
        public void Comma_separated_args()
        {
            Environment.SetEnvironmentVariable("PRUNE_PROTECTED_TAGS", "123,456");
            ISettings settings = NinjectHelper.Get<ISettings>();

            // shoudl revert to default
            Assert.Equal(2, settings.PruneProtectectedTags.Count());

            Assert.Contains("123", settings.PruneProtectectedTags);
            Assert.Contains("456", settings.PruneProtectectedTags);
        }

    }
}
